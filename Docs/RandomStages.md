# 隨機關卡系統

## 設定位置

- `Assets/Resources/StagePools.asset`：各進度的候選池、權重及下一進度。
- `Assets/Resources/StageRooms/`：綁定場景、獎勵與出口規則的關卡方案。
- Inspector 的 Create → ProjectD → Stages 可建立 Room 或 Pool Database。
- Tools → ProjectD → Stages → Validate Pools 檢查設定。

進度與場景分開。例如進度 2（畫面 1-2）的池，可以包含多個使用不同場景的 Room。
玩家選的是完整 Room，進場時不重新抽選場景或獎勵。

## Room 欄位

| 欄位 | 說明 |
| --- | --- |
| Scene Name | Build Settings 中啟用的場景名稱，不含副檔名。 |
| Kind | Combat、Merchant、Boss、Event、Home；Boss 使用現有天賦點與天賦樹獎勵。 |
| Completion | ClearEnemies：清怪後給獎勵、開門。OpenOnEntry：直接開門，不結算戰鬥獎勵。 |
| Reward | 此 Room 的通關獎勵，與場景綁定；一般數量計算沿用 LevelCtrl。 |
| Exit Count | 此關提供的出口數量；不是進入此關的門數量。0 代表沒有出口。 |
| Entrance Prefab | 指向這個 Room 的門外觀，根物件必須有 Door 類型的 Npc。 |
| Entrance Name Key | 現有 I2 多語系 Key；留空沿用門 Prefab 的 Name。 |

Merchant 與 Home 使用 OpenOnEntry、Reward=None。商人、固定商品與其他互動仍在場景內設定。
Event 可自行選擇清怪或直接開門；此版本沒有新增事件任務判定。

## 抽選規則

1. 目前 Room 的 Exit Count 決定抽幾個選項。
2. 從目前進度的 Next Progress Index 對應池抽取；據點改用 First Progress Index。
3. 權重越高越容易抽到；Weight=0 停用該方案。
4. 同一次不重複抽取同一個 Room 資產；同場景但不同獎勵的兩個 Room 是不同方案。
5. 候選不足時減少門數，不補重複選項；空池視為設定錯誤。
6. Next Progress Index=0 回據點，只產生一個回程門。
7. 在出口開放時抽選一次。靠近、切換語言、與門互動都不重抽。
8. 選門後重開目前關卡，保留同一個 Room；新一輪通關會重新抽下一批出口。

## 初始資料

以既有場景與原本進場獎勵為基礎建立，共 13 個進度池、21 個 Room（含據點）。
沒有新增地形，使用同一場景的不同獎勵方案目前只會改變通關獎勵。

| 進度 | 初始候選 | 該關出口數 |
| --- | --- | --- |
| 據點 | 下一池為進度 1 | 1 |
| 1 | Level_1_1＋天賦點 | 2 |
| 2 | Level_2_1＋天賦點／金幣 | 1 |
| 3 | Level_3_1 商人（固定商品） | 2 |
| 4、5 | 各自既有場景＋天賦點／金幣 | 2、1 |
| 6 | Level_6_1 商人（隨機商品） | 2 |
| 7、8 | 各自既有場景＋天賦點／金幣 | 2、1 |
| 9 | Level_9_1 商人（隨機商品） | 1 |
| 10 | Level_10_1 Boss，既有雙獎勵 | 1 |
| 11 | Level_11_1，沿用原本無額外通關獎勵 | 2 |
| 12、13 | 各自既有場景＋天賦點／金幣 | 2、1 |

只有兩個候選且開兩門時，兩種方案都出現，隨機的是門的位置分配。
要讓每次出現不同組合，請在該池放入多於出口數的不同 Room。

## 場景與出口位置

執行時會讀取目前場景內既有 Door Npc 的位置，停用原門，再建立指向抽選結果的新門。
原場景資產與門 Prefab 不會被改寫。增加出口數時，建議也在場景中放足出口位置；
數量不足時，程式會以第一個位置向右每隔 4 單位補位，需自行確認可通行性。
商人關直接開門，一般戰鬥關等必要敵人清空才開門。
木桶與 Minion 不計入清怪條件。ClearEnemies 關卡若沒有必要敵人，會立即完成。

新場景必須加入並啟用於 Build Settings。首次增加 Room 後應執行 Validate Pools。
現有門名稱使用既有 I2 Key，沒有新增翻譯詞條。未來若增加玩家可見文字，仍須同步 CSV、I2 與雲端主表。

## 驗證

- `ProjectDStageValidation.RunBatch`：設定、場景引用與加權不重複抽選檢查。
- `ProjectDStagePlayValidation.RunBatch`：專用 Unity batchmode 遊戲流程回歸；一般編輯器不會自動啟動。
- 驗證日誌放在相鄰 `ProjectD AI/Validation/RandomStages/`。
