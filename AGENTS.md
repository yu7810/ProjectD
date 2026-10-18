# 專案工作規則

## 遊戲文字與多語系雲端同步

- 實作內容時，凡是新增或修改玩家會看到的文字（介面、提示、技能、裝備、天賦、NPC 等），都必須一併更新多語系，並在工作完成前同步到雲端主表。
- 本機文字資料為 `Assets/Localization.csv`；遊戲實際使用 `Assets/Resources/I2Languages.asset`。兩者都要與本次新增或修改的詞條保持一致。
- 雲端主表：<https://docs.google.com/spreadsheets/d/1C_ORywkclOb4Rg58aaSsfy7dnbQCERr83kPZnofz8lo/edit>；分頁為 `Localization`，sheetId 為 `1066662696`。
- 同步前先讀取最新表頭、Key 與目標儲存格。以 Key 定位既有詞條，不依賴先前的列號；新增 Key 時先確認沒有重複。
- 保留既有 Key、其他詞條、欄位格式與公式；簡中欄沿用主表既有的 `GOOGLETRANSLATE` 公式。繁中與英文需提供與實作一致的文字。
- 同步後重新讀取目標列，確認內容、Key、公式結果均正確，再回報完成。若無法連線或沒有寫入權限，明確告知尚未同步，不可宣稱完成。
- 這項常設要求已由使用者授權，後續正常範圍的遊戲文字同步不需再次詢問是否要同步。
