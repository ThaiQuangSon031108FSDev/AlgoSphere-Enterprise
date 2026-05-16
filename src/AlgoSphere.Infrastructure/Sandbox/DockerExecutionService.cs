using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AlgoSphere.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AlgoSphere.Infrastructure.Sandbox;

/// <summary>
/// Real Docker-based code execution sandbox.
/// Wraps user code with a tracing harness, runs in a locked-down container,
/// and returns TraceLog JSON from stdout.
/// </summary>
public class DockerExecutionService : IExecutionService
{
    private readonly ILogger<DockerExecutionService> _logger;

    private const int MaxRuntimeMs = 5000;
    private const int MemLimitMb   = 128;

    private static readonly Dictionary<string, string> Images = new(StringComparer.OrdinalIgnoreCase)
    {
        ["javascript"] = "node:22-alpine",
        ["python"]     = "python:3.12-alpine",
    };

    // Warm Pool: Image -> ContainerId
    private static readonly Dictionary<string, string> ContainerPool = new();
    private static readonly object PoolLock = new();

    public DockerExecutionService(ILogger<DockerExecutionService> logger)
    {
        _logger = logger;
    }

    public async Task<ExecutionResult> ExecuteAsync(string code, string language, string entryPoint, IEnumerable<TestCaseDto> testCases)
    {
        _logger.LogInformation("Executing code with entryPoint {EntryPoint} in {Lang}", entryPoint, language);

        if (!Images.TryGetValue(language, out var image))
            return new ExecutionResult(false, "{}", $"Language '{language}' not supported.", 0, 0);

        var sw = Stopwatch.StartNew();
        try
        {
            var (ok, stdout, stderr) = await RunInDockerAsync(code, language, image, entryPoint, testCases);
            sw.Stop();

            if (!ok)
            {
                _logger.LogWarning("Sandbox error for entryPoint {EntryPoint}: {Err}", entryPoint, stderr);
                return new ExecutionResult(false, "{}", stderr.Trim(), (int)sw.ElapsedMilliseconds, 0);
            }

            var traceJson = stdout.Trim();
            try { JsonDocument.Parse(traceJson); }
            catch
            {
                return new ExecutionResult(false, "{}", "Code produced non-JSON output.", (int)sw.ElapsedMilliseconds, 0);
            }

            return new ExecutionResult(true, traceJson, "OK", (int)sw.ElapsedMilliseconds, MemLimitMb * 1024);
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            return new ExecutionResult(false, "{}", "Time Limit Exceeded (5s).", (int)sw.ElapsedMilliseconds, 0);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Unexpected sandbox error");
            return new ExecutionResult(false, "{}", $"Sandbox error: {ex.Message}", (int)sw.ElapsedMilliseconds, 0);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    private async Task<(bool success, string stdout, string stderr)> RunInDockerAsync(
        string userCode, string language, string image, string entryPoint, IEnumerable<TestCaseDto> testCases)
    {
        var ext = language.ToLower() == "python" ? "py" : "js";
        var instrumentedCode = language.ToLower() switch
        {
            "python"     => BuildPythonTracer(userCode, entryPoint, testCases),
            _            => BuildJsTracer(userCode, entryPoint, testCases),
        };

        var runCmd = ext == "py" ? "python3 -" : "node -";
        var containerId = await GetOrStartWarmContainer(image);

        // Execute in warm container
        using var cts = new CancellationTokenSource(MaxRuntimeMs);
        var result = await RunProcessAsync("docker", $"exec -i {containerId} {runCmd}", instrumentedCode, cts.Token);
        
        // If exec failed because container died, retry once with a fresh one
        if (result.stderr.Contains("is not running") || result.exitCode != 0 && string.IsNullOrEmpty(result.stdout))
        {
            lock(PoolLock) { ContainerPool.Remove(image); }
            containerId = await GetOrStartWarmContainer(image);
            result = await RunProcessAsync("docker", $"exec -i {containerId} {runCmd}", instrumentedCode, cts.Token);
        }

        return (result.exitCode == 0, result.stdout, result.stderr);
    }

    private async Task<string> GetOrStartWarmContainer(string image)
    {
        lock (PoolLock)
        {
            if (ContainerPool.TryGetValue(image, out var id)) return id;
        }

        _logger.LogInformation("Starting warm container for {Image}", image);
        var dockerArgs = string.Join(" ",
            "run", "-d",
            "--memory=128m",
            "--cpus=0.5",
            "--network=none",
            "--read-only",
            "--tmpfs=/tmp:size=16m",
            image,
            "tail -f /dev/null");

        var result = await RunProcessAsync("docker", dockerArgs, null, CancellationToken.None);
        var newId = result.stdout.Trim();

        if (string.IsNullOrEmpty(newId)) 
            throw new Exception($"Failed to start warm container: {result.stderr}");

        lock (PoolLock)
        {
            ContainerPool[image] = newId;
        }

        return newId;
    }

    private static async Task<(int exitCode, string stdout, string stderr)> RunProcessAsync(
        string fileName, string args, string? stdinContent, CancellationToken ct)
    {
        var psi = new ProcessStartInfo(fileName, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            RedirectStandardInput  = stdinContent != null,
            UseShellExecute        = false,
            CreateNoWindow         = true,
        };

        using var process = new Process { StartInfo = psi };
        var stdoutSb = new StringBuilder();
        var stderrSb = new StringBuilder();

        process.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) stdoutSb.AppendLine(e.Data); };
        process.ErrorDataReceived  += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) stderrSb.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (stdinContent != null)
        {
            await process.StandardInput.WriteAsync(stdinContent);
            process.StandardInput.Close();
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

        try
        {
            await process.WaitForExitAsync(combinedCts.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill();
            return (1, "", "Execution Timeout (5s)");
        }

        var fullStdout = stdoutSb.ToString().Trim();
        var success = process.ExitCode == 0;
        var traceLog = fullStdout;

        var startMarker = "###ALGO_START###";
        var endMarker = "###ALGO_END###";
        var startIndex = fullStdout.IndexOf(startMarker);
        var endIndex = fullStdout.LastIndexOf(endMarker);

        if (startIndex >= 0 && endIndex > startIndex)
        {
            traceLog = fullStdout.Substring(startIndex + startMarker.Length, endIndex - startIndex - startMarker.Length);
            if (traceLog.Contains("\"a\":\"err\""))
            {
                success = false;
            }
        }
        else if (!success && !string.IsNullOrEmpty(stderrSb.ToString()))
        {
            // If it failed and we have stderr but no markers, create a fallback error trace
            traceLog = $"{{\"initialState\":[], \"trace\":[{{\"s\":1, \"a\":\"err\", \"t\":[], \"v\":{{\"msg\":\"{stderrSb.ToString().Trim().Replace("\"", "\\\"").Replace("\n", " ")}\"}}}}]}}";
        }

        return (success ? 0 : 1, traceLog, stderrSb.ToString());
    }

    // ── JavaScript Tracer ─────────────────────────────────────────────────────
    private static string BuildJsTracer(string userCode, string entryPoint, IEnumerable<TestCaseDto> testCases)
    {
        // Note: Variable tracking (_v) is done via TracedArray Proxy interceptors.
        // Regex-based instrumentation was removed because it corrupted for-loop headers
        // (e.g. `for (let i = 0; i < n; i++)` → SyntaxError due to extra semicolons).
        var instrumented = userCode;

        var sb = new StringBuilder();
        sb.AppendLine("const _trace = [];");
        sb.AppendLine("const _v = {};");
        sb.AppendLine("let _step = 0;");
        sb.AppendLine("let _nodeIdIdx = 0;");
        sb.AppendLine();
        sb.AppendLine("function createNodeProxy(node, id) {");
        sb.AppendLine("  return new Proxy(node, {");
        sb.AppendLine("    set(target, prop, value) {");
        sb.AppendLine("      if (['next', 'left', 'right'].includes(prop)) {");
        sb.AppendLine("        _trace.push({ s: ++_step, a: 'link', t: [id, value ? value._id : -1], v: { ..._v, prop } });");
        sb.AppendLine("      }");
        sb.AppendLine("      target[prop] = value; return true;");
        sb.AppendLine("    }");
        sb.AppendLine("  });");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("class ListNode {");
        sb.AppendLine("  constructor(val) {");
        sb.AppendLine("    this.val = val; this.next = null; this._id = ++_nodeIdIdx;");
        sb.AppendLine("    _trace.push({ s: ++_step, a: 'node', t: [this._id], v: { ..._v, val } });");
        sb.AppendLine("    return createNodeProxy(this, this._id);");
        sb.AppendLine("  }");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("class TreeNode {");
        sb.AppendLine("  constructor(val) {");
        sb.AppendLine("    this.val = val; this.left = null; this.right = null; this._id = ++_nodeIdIdx;");
        sb.AppendLine("    _trace.push({ s: ++_step, a: 'node', t: [this._id], v: { ..._v, val } });");
        sb.AppendLine("    return createNodeProxy(this, this._id);");
        sb.AppendLine("  }");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("function createTracedArray(arr) {");
        sb.AppendLine("  return new Proxy(arr, {");
        sb.AppendLine("    get(target, prop) {");
        sb.AppendLine("      if (typeof prop === 'string' && !isNaN(Number(prop))) {");
        sb.AppendLine("        _trace.push({ s: ++_step, a: 'cmp', t: [Number(prop)], v: { ..._v } });");
        sb.AppendLine("      }");
        sb.AppendLine("      return target[prop];");
        sb.AppendLine("    },");
        sb.AppendLine("    set(target, prop, value) {");
        sb.AppendLine("      target[prop] = value;");
        sb.AppendLine("      if (typeof prop === 'string' && !isNaN(Number(prop))) {");
        sb.AppendLine("        _trace.push({ s: ++_step, a: 'swp', t: [Number(prop)], v: { val: value, ..._v } });");
        sb.AppendLine("      }");
        sb.AppendLine("      return true;");
        sb.AppendLine("    }");
        sb.AppendLine("  });");
        sb.AppendLine("}");
        sb.AppendLine();
        
        var testCaseJson = testCases.FirstOrDefault()?.InputJson ?? "[]";
        
        sb.AppendLine($"const _entryPoint = '{entryPoint}';");
        sb.AppendLine($"const _testCaseInput = {testCaseJson};");
        sb.AppendLine("let _initial = [];");
        sb.AppendLine();
        sb.AppendLine("function prepareArgs(args, entryPoint) {");
        sb.AppendLine("  let isNode = entryPoint.toLowerCase().includes('list');");
        sb.AppendLine("  let firstArrIdx = args.findIndex(a => Array.isArray(a));");
        sb.AppendLine("  ");
        sb.AppendLine("  if (firstArrIdx !== -1) {");
        sb.AppendLine("    _initial = [...args[firstArrIdx]];");
        sb.AppendLine("    if (isNode) {");
        sb.AppendLine("       let dummy = new ListNode(0);");
        sb.AppendLine("       let curr = dummy;");
        sb.AppendLine("       for(let val of args[firstArrIdx]) {");
        sb.AppendLine("           curr.next = new ListNode(val);");
        sb.AppendLine("           curr = curr.next;");
        sb.AppendLine("       }");
        sb.AppendLine("       args[firstArrIdx] = dummy.next;");
        sb.AppendLine("    } else {");
        sb.AppendLine("       args[firstArrIdx] = createTracedArray(args[firstArrIdx]);");
        sb.AppendLine("    }");
        sb.AppendLine("  }");
        sb.AppendLine("  return args;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("class TracedSet extends Set {");
        sb.AppendLine("  add(v) { _trace.push({ s:++_step, a:'vis', t:[], v:{..._v, ds:{name:'Set', op:'add', val:v}} }); return super.add(v); }");
        sb.AppendLine("  has(v) { _trace.push({ s:++_step, a:'vis', t:[], v:{..._v, ds:{name:'Set', op:'has', val:v}} }); return super.has(v); }");
        sb.AppendLine("}");
        sb.AppendLine("global.Set = TracedSet;");
        sb.AppendLine();
        sb.AppendLine("try {");
        sb.AppendLine(instrumented);
        sb.AppendLine("  if (typeof global[_entryPoint] === 'function' || typeof eval(_entryPoint) === 'function') {");
        sb.AppendLine("    let func = typeof global[_entryPoint] === 'function' ? global[_entryPoint] : eval(_entryPoint);");
        sb.AppendLine("    let args = prepareArgs(_testCaseInput, _entryPoint);");
        sb.AppendLine("    func.apply(null, args);");
        sb.AppendLine("  }");
        sb.AppendLine("  process.stdout.write('###ALGO_START###' + JSON.stringify({ initialState: [..._initial], trace: _trace, algorithm: 'javascript' }) + '###ALGO_END###');");
        sb.AppendLine("} catch(e) {");
        sb.AppendLine("  process.stdout.write('###ALGO_START###' + JSON.stringify({ initialState: [..._initial], trace: [{s:1, a:'err', t:[], v:{msg:e.message}}] }) + '###ALGO_END###');");
        sb.AppendLine("}");
        return sb.ToString();
    }

    // ── Python Tracer ─────────────────────────────────────────────────────────
    private static string BuildPythonTracer(string userCode, string entryPoint, IEnumerable<TestCaseDto> testCases)
    {
        // Simple Python instrumentation: append _v['i'] = i after assignments
        var trackedVars = new[] { "i", "j", "left", "right", "low", "high", "mid", "pivot", "temp" };
        var lines = userCode.Split('\n');
        var instrumented = new StringBuilder();
        foreach (var line in lines)
        {
            instrumented.AppendLine(line);
            var trimmed = line.TrimStart();
            foreach (var v in trackedVars)
            {
                if (trimmed.StartsWith($"{v} =") || trimmed.StartsWith($"{v} +=") || trimmed.StartsWith($"{v} -="))
                {
                    // Match indentation
                    var indent = line.Substring(0, line.Length - trimmed.Length);
                    instrumented.AppendLine($"{indent}_v['{v}'] = {v}");
                }
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("import json, sys");
        sb.AppendLine("_trace = []");
        sb.AppendLine("_v = {}");
        sb.AppendLine("_step = [0]");
        sb.AppendLine();
        sb.AppendLine("def _cmp(arr, i, j):");
        sb.AppendLine("    _step[0] += 1");
        sb.AppendLine("    _trace.append({'s': _step[0], 'a': 'cmp', 't': [i, j], 'v': dict(_v)})");
        sb.AppendLine("    return arr[i] > arr[j]");
        sb.AppendLine();
        sb.AppendLine("def _swp(arr, i, j):");
        sb.AppendLine("    arr[i], arr[j] = arr[j], arr[i]");
        sb.AppendLine("    _step[0] += 1");
        sb.AppendLine("    _trace.append({'s': _step[0], 'a': 'swp', 't': [i, j], 'v': dict(_v)})");
        var testCaseJson = testCases.FirstOrDefault()?.InputJson ?? "[]";

        sb.AppendLine();
        sb.AppendLine("# ── User code ─────────────────────────────────────");
        sb.AppendLine(instrumented.ToString());
        sb.AppendLine("# ──────────────────────────────────────────────────");
        sb.AppendLine();
        sb.AppendLine($"_test_case_input = json.loads('{testCaseJson}')");
        sb.AppendLine($"_entry_point = '{entryPoint}'");
        sb.AppendLine("_initial = []");
        sb.AppendLine();
        sb.AppendLine("try:");
        sb.AppendLine("    # Find first array in args to trace");
        sb.AppendLine("    for i, arg in enumerate(_test_case_input):");
        sb.AppendLine("        if isinstance(arg, list):");
        sb.AppendLine("            _initial = list(arg)");
        sb.AppendLine("            break");
        sb.AppendLine("    ");
        sb.AppendLine("    if _entry_point in globals() and callable(globals()[_entry_point]):");
        sb.AppendLine("        globals()[_entry_point](*_test_case_input)");
        sb.AppendLine("except Exception as e:");
        sb.AppendLine("    _trace.append({'s': _step[0]+1, 'a': 'err', 't': [], 'v': {'msg': str(e)}})");
        sb.AppendLine();
        sb.AppendLine("sys.stdout.write(json.dumps({'initialState': _initial, 'trace': _trace, 'algorithm': 'python'}))");
        return sb.ToString();
    }
}
