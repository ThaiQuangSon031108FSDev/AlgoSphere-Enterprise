import { test, expect } from '@playwright/test';

test.describe('Happy Path: From Login to Success', () => {
  test('nên cho phép người dùng đăng nhập, chọn bài và chạy code thành công', async ({ page }) => {
    // 1. Đi tới trang login
    await page.goto('/login');

    // 2. Thực hiện đăng nhập (Sử dụng ID đã thêm)
    await page.fill('#login-username', 'student');
    await page.fill('#login-password', 'Password123!');
    await page.click('#login-submit');

    // 3. Chờ chuyển hướng tới Dashboard/Skill Tree
    await expect(page).toHaveURL(/.*dashboard|skill-tree/);

    // 4. Tìm và click vào Topic "Bubble Sort" trong Skill Tree
    // Chúng ta sử dụng data-topic-id="1" (giả sử ID là 1 cho Bubble Sort)
    const bubbleSortNode = page.locator('[data-topic-id="1"]');
    await expect(bubbleSortNode).toBeVisible();
    await bubbleSortNode.click();

    // 5. Chờ chuyển hướng tới Workspace
    await expect(page).toHaveURL(/.*workspace\/1/);

    // 6. Click Run Code
    const runBtn = page.locator('#run-code-btn');
    await expect(runBtn).toBeVisible();
    await runBtn.click();

    // 7. Kiểm tra kết quả: Có Overlay thông báo thành công (GSAP animation trigger)
    // Giả sử có class .success-overlay hoặc check AI Mentor message
    await expect(page.locator('text=Code đã chạy!')).toBeVisible({ timeout: 15000 });
    
    // Kiểm tra xem có nhận được XP không
    await expect(page.locator('text=XP')).toBeVisible();
  });
});
