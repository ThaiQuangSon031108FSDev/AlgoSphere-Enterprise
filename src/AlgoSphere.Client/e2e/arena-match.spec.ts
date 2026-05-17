import { test, expect } from '@playwright/test';

test.describe('Arena Matchmaking & Real-time Sync Flow', () => {
  test.beforeEach(async ({ page }) => {
    // 1. Mock Login auth token
    await page.addInitScript(() => {
      window.localStorage.setItem('token', 'mock-token-arena');
    });
  });

  test('nên hiển thị giao diện tìm trận Arena và các nút tương tác', async ({ page }) => {
    // 1. Đi tới trang Arena
    await page.goto('/arena');

    // 2. Chờ tiêu đề Arena hiển thị
    await expect(page.locator('text=CODE ARENA').first()).toBeVisible();

    // 3. Kiểm tra nút bắt đầu ghép cặp
    const startBtn = page.locator('text=BẮT ĐẦU GHÉP CẶP');
    await expect(startBtn).toBeVisible();

    // 4. Click tìm trận
    await startBtn.click();

    // 5. Nút hủy tìm trận sẽ hiện ra
    const cancelBtn = page.locator('text=Hủy tìm trận');
    await expect(cancelBtn).toBeVisible();

    // 6. Click hủy tìm trận
    await cancelBtn.click();
    
    // 7. Quay lại nút Bắt đầu
    await expect(startBtn).toBeVisible();
  });
});
