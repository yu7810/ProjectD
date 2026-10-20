using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public partial class UICtrl
{
    // Option 0 is the existing empty placeholder. Values map to progress IDs,
    // never to translated text or an assumed contiguous dropdown index.
    private readonly List<int> stageDropdownIndices = new List<int>();

    private void OnEnable()
    {
        LocalizationManager.OnLocalizeEvent += RefreshStageDropdown;
    }

    private void OnDisable()
    {
        LocalizationManager.OnLocalizeEvent -= RefreshStageDropdown;
    }

    private void RefreshStageDropdown()
    {
        if (ClassDropdown == null) return;
        StagePoolDatabase database = LevelCtrl.Instance != null ? LevelCtrl.Instance.StagePools :
            Resources.Load<StagePoolDatabase>("StagePools");
        var labels = new List<string> { "" };
        stageDropdownIndices.Clear();
        stageDropdownIndices.Add(-1);
        LocalizationManager.TryGetTranslation("UI/Home", out string home);
        labels.Add(home);
        stageDropdownIndices.Add(0);
        if (database != null)
        {
            var indices = new SortedSet<int>();
            foreach (StagePool pool in database.pools)
                if (pool != null && pool.progressIndex > 0) indices.Add(pool.progressIndex);
            foreach (int progress in indices)
            {
                int chapter = (progress - 1) / 10 + 1;
                int stage = (progress - 1) % 10 + 1;
                labels.Add(chapter + "-" + stage);
                stageDropdownIndices.Add(progress);
            }
        }
        // Reopening the menu must also allow selecting the same destination again.
        ClassDropdown.SetValueWithoutNotify(0);
        ClassDropdown.ClearOptions();
        ClassDropdown.AddOptions(labels);
        ClassDropdown.SetValueWithoutNotify(0);
        ClassDropdown.RefreshShownValue();
    }
}
