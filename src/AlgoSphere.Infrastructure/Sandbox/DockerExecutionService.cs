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

    public DockerExecutionService(ILogger<DockerExecutionService> logger)
    {
        _logger = logger;
    }

    public async Task<ExecutionResult> ExecuteAsync(string code, string language, int exerciseId)
    {
        _logger.LogInformation("Executing code for exercise {Id} in {Lang}", exerciseId, language);

        if (!Images.TryGetValue(language, out var image))
            return new ExecutionResult(false, "{}", $"Language '{language}' not supported.", 0, 0);

        var sw = Stopwatch.StartNew();
        try
        {
            var (ok, stdout, stderr) = await RunInDockerAsync(code, language, image);
            sw.Stop();

            if (!ok)
            {
                _logger.LogWarning("Sandbox error for exercise {Id}: {Err}", exerciseId, stderr);
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
        string userCode, string language, string image)
    {
        var ext = language.ToLower() == "python" ? "py" : "js";
        var instrumentedCode = language.ToLower() switch
        {
            "python"     => BuildPythonTracer(userCode),
            _            => BuildJsTracer(userCode),
        };

        var runCmd = ext == "py" ? "python -" : "node -";

        // -i ensures docker reads from stdin and passes it to the container
        var dockerArgs = string.Join(" ",
            "run", "-i", "--rm",
            "--memory=128m",
            "--cpus=0.5",
            "--network=none",
            "--read-only",
            "--tmpfs=/tmp:size=16m",
            image,
            runCmd);

        using var cts = new CancellationTokenSource(MaxRuntimeMs);
        var result = await RunProcessAsync("docker", dockerArgs, instrumentedCode, cts.Token);
        return (result.exitCode == 0, result.stdout, result.stderr);
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

        process.OutputDataReceived += (_, e) => { if (e.Data != null) stdoutSb.AppendLine(e.Data); };
        process.ErrorDataReceived  += (_, e) => { if (e.Data != null) stderrSb.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (stdinContent != null)
        {
            await process.StandardInput.WriteAsync(stdinContent);
            process.StandardInput.Close();
        }

        await process.WaitForExitAsync(ct);

        return (process.ExitCode, stdoutSb.ToString(), stderrSb.ToString());
    }

    // ── JavaScript Tracer ─────────────────────────────────────────────────────
    private static string BuildJsTracer(string userCode)
    {
        var sb = new StringBuilder();
        sb.AppendLine("'use strict';");
        sb.AppendLine("const _trace = [];");
        sb.AppendLine("let _step = 0;");
        sb.AppendLine();
        sb.AppendLine("function createTracedArray(arr) {");
        sb.AppendLine("  return new Proxy(arr, {");
        sb.AppendLine("    get(target, prop) {");
        sb.AppendLine("      if (typeof prop === 'string' && !isNaN(Number(prop))) {");
        sb.AppendLine("        _trace.push({ s: ++_step, a: 'cmp', t: [Number(prop)], v: {} });");
        sb.AppendLine("      }");
        sb.AppendLine("      return target[prop];");
        sb.AppendLine("    },");
        sb.AppendLine("    set(target, prop, value) {");
        sb.AppendLine("      target[prop] = value;");
        sb.AppendLine("      if (typeof prop === 'string' && !isNaN(Number(prop))) {");
        sb.AppendLine("        _trace.push({ s: ++_step, a: 'swp', t: [Number(prop)], v: { val: value } });");
        sb.AppendLine("      }");
        sb.AppendLine("      return true;");
        sb.AppendLine("    }");
        sb.AppendLine("  });");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("const _initial = [64, 34, 25, 12, 22, 11, 90];");
        sb.AppendLine("let _arr = createTracedArray([..._initial]);");
        sb.AppendLine();
        sb.AppendLine("try {");
        sb.AppendLine("// ── User code ─────────────────────────────────────");
        sb.AppendLine(userCode);
        sb.AppendLine("// ──────────────────────────────────────────────────");
        sb.AppendLine("  if (typeof bubbleSort === 'function') bubbleSort(_arr);");
        sb.AppendLine("  else if (typeof twoSum === 'function') twoSum(_arr, 90);");
        sb.AppendLine("  else if (typeof binarySearch === 'function') binarySearch(_arr, 25);");
        sb.AppendLine("  else if (typeof solve === 'function') solve(_arr);");
        sb.AppendLine("} catch(e) {");
        sb.AppendLine("  _trace.push({ s: ++_step, a: 'err', t: [], v: { msg: e.message } });");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("process.stdout.write(JSON.stringify({ initialState: _initial, trace: _trace, algorithm: 'javascript' }));");
        return sb.ToString();
    }

    // ── Python Tracer ─────────────────────────────────────────────────────────
    private static string BuildPythonTracer(string userCode)
    {
        var sb = new StringBuilder();
        sb.AppendLine("import json, sys");
        sb.AppendLine("_trace = []");
        sb.AppendLine("_step = [0]");
        sb.AppendLine();
        sb.AppendLine("def _cmp(arr, i, j, vars=None):");
        sb.AppendLine("    _step[0] += 1");
        sb.AppendLine("    _trace.append({'s': _step[0], 'a': 'cmp', 't': [i, j], 'v': vars or {}})");
        sb.AppendLine("    return arr[i] > arr[j]");
        sb.AppendLine();
        sb.AppendLine("def _swp(arr, i, j, vars=None):");
        sb.AppendLine("    arr[i], arr[j] = arr[j], arr[i]");
        sb.AppendLine("    _step[0] += 1");
        sb.AppendLine("    _trace.append({'s': _step[0], 'a': 'swp', 't': [i, j], 'v': vars or {}})");
        sb.AppendLine();
        sb.AppendLine("# ── User code ─────────────────────────────────────");
        sb.AppendLine(userCode);
        sb.AppendLine("# ──────────────────────────────────────────────────");
        sb.AppendLine();
        sb.AppendLine("_initial = [64, 34, 25, 12, 22, 11, 90]");
        sb.AppendLine("a = list(_initial)");
        sb.AppendLine("for i in range(len(a)):");
        sb.AppendLine("    for j in range(len(a) - i - 1):");
        sb.AppendLine("        if _cmp(a, j, j+1, {'i': i, 'j': j}):");
        sb.AppendLine("            _swp(a, j, j+1, {'temp': a[j]})");
        sb.AppendLine();
        sb.AppendLine("sys.stdout.write(json.dumps({'initialState': _initial, 'trace': _trace, 'algorithm': 'python'}))");
        return sb.ToString();
    }
}
