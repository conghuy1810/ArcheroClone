using System.Collections;
using System.Collections.Generic;
using TestTask.Helper;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using TestTask.Core;

public class RewardSystem : MonoBehaviour
{
    // Lưu tên của phần thưởng người chơi có thể chọn
    public List<string> rewardsTitle = new List<string>();

    // Cả 3 thành phần Text hiển thị phần thưởng trên UI
    [SerializeField] Text[] rewards;

    List<string> rewardChoice = new List<string>();

    [Header("Khoảng thời gian giữa mỗi lần đổi tên khi cuộn")]
    [SerializeField] float time = 0.15f;

    // Khi biến này bằng true mới cho phép người chơi click chọn phần thưởng
    bool selectReward = false;
    PlayerSkill playerSkill;
    List<PlayerSkill.SkillType> skill = new List<PlayerSkill.SkillType>();
    PlayerSkill.SkillType[] selectedSkill = new PlayerSkill.SkillType[3];

    GameObject[] weapons = new GameObject[3];

    private void Awake() {
        // Chuyển đổi tên các enum SkillType thành danh sách chuỗi
        rewardsTitle = Enum.GetNames(typeof(PlayerSkill.SkillType)).ToList();
        // Chuyển đổi các giá trị enum SkillType thành danh sách
        skill = Enum.GetValues(typeof(PlayerSkill.SkillType)).Cast<PlayerSkill.SkillType>().ToList();
    }

    private void OnEnable() {
        // Dừng hoàn toàn trò chơi (ngưng quái vật, đạn bắn) khi bảng chọn quà xuất hiện
        Time.timeScale = 0f;
        GameHandler.instance.isPause = true;
        
        // Bắt đầu chạy hiệu ứng cuộn xoay thưởng bằng Coroutine thời gian thực (không bị ảnh hưởng bởi Time.timeScale = 0)
        StartCoroutine(ScrollRewardsRoutine());
    }

    private IEnumerator ScrollRewardsRoutine()
    {
        selectReward = false;
        float elapsed = 0f;
        float totalScrollDuration = 1.5f; // Thời gian cuộn chữ (giảm từ 3s xuống 1.5s để người chơi đỡ phải chờ lâu)

        while (elapsed < totalScrollDuration)
        {
            ShowRewards();
            // Sử dụng thời gian thực thực tế
            yield return new WaitForSecondsRealtime(time);
            elapsed += time;
        }

        // Hiện kết quả cuối cùng và cho phép click chọn
        ShowRewards();
        selectReward = true;
    }

    public void UnlockSkill(int i)
    {
        if (!selectReward) return;

        // Mở khóa kỹ năng đã chọn
        playerSkill.UnlockSkill(selectedSkill[i]);
        gameObject.SetActive(false);

        // Khôi phục lại thời gian và tiếp tục trò chơi
        Time.timeScale = 1f;
        GameHandler.instance.isPause = false;
    }

    public void SetPlayerSkill(PlayerSkill playerSkill)
    {
        this.playerSkill = playerSkill;
    }

    /// <summary>
    /// Hiển thị các kỹ năng ngẫu nhiên lên bảng UI chọn thưởng
    /// </summary>
    void ShowRewards()
    {
         List<string> reward = rewardsTitle.ToList();
         List<PlayerSkill.SkillType> skills = skill.ToList();
         for (int i = 0; i < rewards.Length; i++){
             int randomIntWithinRange = UnityEngine.Random.Range(0, reward.Count);
             String currentReward =  reward[randomIntWithinRange];
             rewards[i].text = currentReward;
             selectedSkill[i] = skills[randomIntWithinRange];
             
             // Loại bỏ phần tử đã chọn để tránh trùng lặp giữa 3 ô
             reward.Remove(currentReward);
             skills.Remove(skills[randomIntWithinRange]);
         }
    }
}
