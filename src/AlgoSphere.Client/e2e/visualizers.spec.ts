import { test, expect } from '@playwright/test';

test.describe('Visualizer Engine Render Integrity', () => {
  test.beforeEach(async ({ page }) => {
    // 1. Mock Login auth token
    await page.addInitScript(() => {
      window.localStorage.setItem('token', 'mock-token-abc');
    });
  });

  test('nên hiển thị TwoPointerRenderer khi chạy thuật toán con trỏ', async ({ page }) => {
    // Mock sandbox execute API
    await page.route('**/exercises/execute', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          execution: {
            success: true,
            traceLog: JSON.stringify({
              initialState: [10, 20, 30],
              trace: [
                { s: 0, l: 2, a: 'ptr', t: [0], v: { name: 'left', val: 10 } },
                { s: 1, l: 3, a: 'ptr', t: [2], v: { name: 'right', val: 30 } }
              ]
            })
          },
          gamification: { xpEarned: 10, bonusXp: 0, isLevelUp: false }
        })
      });
    });

    // Go directly to Workspace 1
    await page.goto('/workspace/1');

    // Click run code to invoke visualizer state updates
    const runBtn = page.locator('#run-code-btn');
    await expect(runBtn).toBeVisible();
    await runBtn.click();

    // Step forward to activate trace event (ptr)
    const stepFwdBtn = page.locator('[title="Bước tiến"]');
    await expect(stepFwdBtn).toBeVisible();
    await stepFwdBtn.click(); // Step 0 (left pointer)

    // Verify TwoPointer visualizer renders pointers info
    const pointerVisualizer = page.locator('.two-pointer-visualizer');
    await expect(pointerVisualizer).toBeVisible();
    await expect(page.locator('text=Two-Pointer / Sliding Window')).toBeVisible();
    await expect(pointerVisualizer.locator('text=LEFT:')).toBeVisible();

    await stepFwdBtn.click(); // Step 1 (right pointer)
    await expect(pointerVisualizer.locator('text=RIGHT:')).toBeVisible();
  });

  test('nên hiển thị HashMapRenderer khi băm key-value', async ({ page }) => {
    await page.route('**/exercises/execute', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          execution: {
            success: true,
            traceLog: JSON.stringify({
              initialState: [],
              trace: [
                { s: 0, l: 2, a: 'hset', t: [], v: { name: 'myMap', key: 'name', val: 'son' } }
              ]
            })
          },
          gamification: { xpEarned: 10, bonusXp: 0 }
        })
      });
    });

    await page.goto('/workspace/1');
    await page.click('#run-code-btn');

    // Step forward to load HashMap key-value state
    const stepFwdBtn = page.locator('[title="Bước tiến"]');
    await expect(stepFwdBtn).toBeVisible();
    await stepFwdBtn.click();

    const mapVisualizer = page.locator('.hashmap-visualizer');
    await expect(mapVisualizer).toBeVisible();
    await expect(page.locator('text=HashMap (Buckets)')).toBeVisible();
    await expect(mapVisualizer.locator('text=name')).toBeVisible();
    await expect(mapVisualizer.locator('text=son')).toBeVisible();
  });

  test('nên hiển thị DPTableRenderer khi duyệt quy hoạch động', async ({ page }) => {
    await page.route('**/exercises/execute', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          execution: {
            success: true,
            traceLog: JSON.stringify({
              initialState: [],
              trace: [
                { s: 0, l: 2, a: 'dpset', t: [0, 0], v: { val: 5, formula: 'dp[0][0]=5' } }
              ]
            })
          },
          gamification: { xpEarned: 10, bonusXp: 0 }
        })
      });
    });

    await page.goto('/workspace/1');
    await page.click('#run-code-btn');

    // Step forward to load DPTable state
    const stepFwdBtn = page.locator('[title="Bước tiến"]');
    await expect(stepFwdBtn).toBeVisible();
    await stepFwdBtn.click();

    const dpVisualizer = page.locator('.dp-table-visualizer');
    await expect(dpVisualizer).toBeVisible();
    await expect(page.locator('text=Dynamic Programming Table')).toBeVisible();
    await expect(dpVisualizer.locator('.font-mono.text-slate-200').first()).toHaveText('5');
  });
});
