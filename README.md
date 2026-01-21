# 🎁 בונוסים שמומשו בפרויקט

## שימוש ב-C#

### ✓ שימוש נכון ומלא ב-TryParse בתוכניות בדיקה (1 נק')

שימוש ב-TryParse עם בדיקה של ערך מוחזר ומשתנה מוגדר בתוך הזימון. 

**מימושים:**
- [`DalXml/XmlTools. cs` - `ToDoubleNullable`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/DalXml/XmlTools.cs#L150-L155) - `double. TryParse` עם בדיקת ערך מוחזר
- [`DalXml/XmlTools.cs` - `ToIntNullable`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/DalXml/XmlTools. cs#L145-L150) - `int.TryParse` עם משתנה מוגדר בזימון
- [`DalXml/XmlTools.cs` - `ToDateTimeNullable`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/DalXml/XmlTools. cs#L156-L161) - `DateTime.TryParse`
- [`DalXml/XmlTools.cs` - `ToTimeSpanNullable`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/DalXml/XmlTools.cs#L162-L167) - `TimeSpan.TryParse`
- [`DalXml/XmlTools.cs` - `ToEnumNullable`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/DalXml/XmlTools.cs#L168-L173) - `Enum.TryParse`

שימוש בכל Extension Methods ברחבי הפרויקט לטעינת נתונים מ-XML ��צורה בטוחה. 

---

## שכבת נתונים (DAL)

### ✓ הוספת תכונת סיסמא (2 נק')

תכונת סיסמה בכל שכבות הפרויקט מ-DO עד PL. 

**מימושים:**
- [`DalFacade/DO/Courier.cs`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/DalFacade/DO/Courier.cs#L8) - `string Password` בישות DO
- [`BL/BO/Courier.cs`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/BL/BO/Courier.cs#L37) - `required string Password` בישות BO
- [`DalXml/CourierImplementation.cs` - getCourier](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/DalXml/CourierImplementation.cs#L15-L32) - קריאת Password מ-XML
- [`DalList/ConfigImplementation.cs`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/DalList/ConfigImplementation.cs#L13-L20) - Password למנהל

### ✓ Singleton עם Thread Safe ו-Lazy Initialization (2 נק')

DAL מיושם כ-Singleton עם Lazy Initialization שמבטיח שלא יהיו מרובים אובייקטים וחוט-בטוח.

**מימושים:**
- [`DalXml/DalXml.cs`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/DalXml/DalXml. cs#L12-L25) - `private static readonly Lazy<IDal> lazyInstance` עם `Instance` property
  - `Lazy<T>` מטפל בـ Thread Safety אוטומטי
  - `lazyInstance. Value` מבצע Lazy Initialization בעת הקריאה הראשונה
  - אין אפשרות ליצור instance חוזר

---

## שכבת הלוגיקה (BL)

### ✓ סיסמה ראשונית ועדכון (1 נק')

מנהל קובע סיסמה ראשונית לשליח, והשליח יכול לעדכן אותה. 

**מימושים:**
- [`BL/Helpers/CourierManager.cs` - UpdateCourier](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/BL/Helpers/CourierManager. cs#L270-L289) - הצפנת סיסמה חדשה עם `Tools. Encrypt()`
- [`PL/Courier/MainCourierWindow.xaml. cs` - BtnUpdate_Click](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/Courier/MainCourierWindow. xaml.cs#L222-L233) - עדכון סיסמה דרך הממשק
- [`BL/Helpers/CourierManager.cs` - DOToBOCourier](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/BL/Helpers/CourierManager.cs#L65-L76) - פענוח סיסמה בעת הוצאה מה-DB

### ✓ בדיקה שהסיסמא חזקה (1 נק')

סיסמה חייבת להכיל:  8 תווים מינימום, אות גדולה, אות קטנה וספרה. 

**מימוש:**
- [`BL/Helpers/CourierManager.cs` - CheckPassword](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/BL/Helpers/CourierManager.cs#L186-L196)
  - בדיקת אורך:  `password.Length < 8`
  - בדיקת Uppercase: `!password.Any(char.IsUpper)`
  - בדיקת Lowercase:  `!password.Any(char.IsLower)`
  - בדיקת Digit: `!password.Any(char.IsDigit)`

### ✓ הצפנת סיסמא עם AES-256 (2 נק')

סיסמאות מוצפנות בשמירה לבסיס הנתונים באמצעות AES-256.

**מימושים:**
- [`BL/Helpers/Tools.cs` - Encrypt](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/BL/Helpers/Tools.cs#L478-L507)
  - Key: 32 תווים (256-bit)
  - IV: zero-filled array בגודל 16
  - Encoding: Base64 לשמירה
- [`BL/Helpers/Tools.cs` - Decrypt](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/BL/Helpers/Tools.cs#L510-L548)
  - פענוח בעזרת אותו Key ו-IV
  - תמיכה בחוזרות לנתונים ישנים (fallback)
- [`BL/Helpers/CourierManager.cs` - AddCourier](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/BL/Helpers/CourierManager. cs#L236-L260) - הצפנה לפני שמירה
- [`BL/Helpers/Tools.cs` - CheckPasswordEntry](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/BL/Helpers/Tools.cs#L378-L398) - פענוח בעת בדיקת כניסה

---

## שכבת התצוגה (PL)

### ✓ תצוגה גרפית אינטראקטיבית עם שינוי צבעים (1 נק')

הממשק משתנה בזמן אמת בתגובה לטעינת קואורדינטות מרשת.

**מימושים:**
- [`PL/NetworkAwareWindow.cs` - ExecuteNetworkActionAsync](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/NetworkAwareWindow.cs#L37-L97)
  - **כתום (Loading):** `AddressBorderBrush = Brushes.Orange;` + `Mouse.OverrideCursor = Cursors.Wait;`
  - **ירוק (הצלחה):** `AddressBorderBrush = Brushes.Green;`
  - **אדום (שגיאה):** `AddressBorderBrush = Brushes.Red;`
- [`PL/Order/OrderWindow.xaml.cs` - BtnAction_Click](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/Order/OrderWindow.xaml. cs#L67-L98) - שימוש ב-ExecuteNetworkActionAsync

### ✓ אייקון של האפליקציה (1 נק')

סמל מופיע בכותרת החלון ובשורת המשימות בכל החלונות.

**מימושים:**
- [`PL/MainWindow.xaml`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/MainWindow.xaml#L5) - `Icon="/Images/logistics-delievry.png"`
- [`PL/LoginWindow.xaml`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/LoginWindow.xaml#L9) - `Icon="/Images/logistics-delievry.png"`
- [`PL/Courier/CourierWindow.xaml`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/Courier/CourierWindow.xaml#L13) - `Icon="/Images/logistics-delievry.png"`
- [`PL/Order/OrderWindow.xaml`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/Order/OrderWindow. xaml#L14) - `Icon="/Images/logistics-delievry.png"`

✓ ENTER שווה ללחיצה על כפתור (1 נק')
בכל חלונות היישום, לחיצת ENTER מקדמת לחיצה על הכפתור הברירת מחדל בחלון.

מימושים:

PL/LoginWindow.xaml - IsDefault="True" על כפתור Login
PL/Order/OrderWindow.xaml - IsDefault="True" על כפתור Action
PL/Courier/CourierWindow.xaml - IsDefault="True" על כפתור Action
PL/Courier/OrdersHistory.xaml - IsDefault="True" על כפתור Close
PL/Order/OrdersToPick.xaml - IsDefault="True" על כפתור Pick Order
PL/Courier/CourierListWindow.xaml - IsDefault="True" על כפתור
PL/Order/OrderInListWindow.xaml - IsDefault="True" על כפתור
PL/MainWindow.xaml - IsDefault="True" על כפתור ברירת מחדל

### ✓ שימוש בטריגרים - DataTrigger (1 נק')

שינוי מצב פקדים בהתאם לנתונים. 

**מימושים:**
- [`PL/MainWindow.xaml` - SimulatedButtonStyle](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/MainWindow.xaml#L11-L21)
  - `<DataTrigger Binding="{Binding IsSimulatorRunning}" Value="True">`
  - משבית כפתורים בעת הרצת סימולטור
- [`PL/Courier/CourierWindow.xaml` - OrderDetailsVisibilityStyle](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/Courier/CourierWindow.xaml#L24-L31)
  - `<DataTrigger Binding="{Binding CurrentCourier.OrderInCare}" Value="{x:Null}">`
  - מסתיר פרטי הזמנה כשאין הזמנה פעילה

### ✓ קיבוץ רשימות לפי סטטוס (2 נק')

רשימת הזמנות מקובצת בהיררכיה לפי סוג או סטטוס.

**מימוש:**
- [`PL/Order/OrderInListWindow.xaml`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/Order/OrderInListWindow.xaml#L58-L66)
  - `<ListView.GroupStyle>` עם `PropertyGroupDescription(nameof(BO.OrderInList.OrderStatus))`
- [`PL/Order/OrderInListWindow.xaml. cs` - queryOrderList](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/Order/OrderInListWindow.xaml. cs#L139-L172)
  - `ListCollectionView view = new ListCollectionView(rawList. ToList());`
  - `view.GroupDescriptions.Add(new PropertyGroupDescription(... ));`

### ✓ סיסמה מוסתרת עם Toggle (1 נק')

הצגת סיסמה רק בלחיצה על כפתור "עין". 

**מימושים:**
- [`PL/LoginWindow.xaml`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/LoginWindow.xaml#L28-L37)
  - `PasswordBox` - מציג כוכביות
  - `TextBox` - הצגת טקסט פתוח
  - `Button` עם `👁` למעבר בין המצבים
- [`PL/LoginWindow.xaml.cs` - BtnTogglePassword_Click](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/LoginWindow.xaml.cs#L40-L57)
  - החלפת `Visibility` בין `PasswordBox` ל-`TextBox`
- [`PL/Courier/MainCourierWindow.xaml. cs` - BtnTogglePassword_Click](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/Courier/MainCourierWindow.xaml.cs#L262-L276)

### ✓ כפתור Delete בתנאי (2 נק')

כפתור Delete בהצעות רשימה משנה את זמינותו בהתאם לתנאים.

**מימוש:**
- [`PL/Courier/CourierListWindow.xaml. cs` - BtnDelete_Click](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/Courier/CourierListWindow.xaml.cs#L145-L173)
  - בדיקה אם אפשר למחוק את השליח (אין הזמנות פעילות)
  - הצגת הודעת שגיאה אם לא ניתן למחוק
  - ריענון הרשימה בעת הצלחה

### ✓ הודעה על כישלון טעינת קואורדינטות (2 נק')

תצוגה של סיבה מפורשת כאשר טעינת קואורדינטות נכשלה.

**מימושים:**
- [`PL/NetworkAwareWindow.cs` - ExecuteNetworkActionAsync](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/NetworkAwareWindow. cs#L62-L80)
  - **Network Error:** `"Network Error:  {ex.Message}"`
  - **Invalid Address:** `"Invalid Address: Could not find coordinates. "`
  - **System Error:** `"System Error: {ex.Message}"`
- [`PL/Order/OrderWindow.xaml.cs`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/Order/OrderWindow.xaml.cs#L1-L206) - שימוש ב-ExecuteNetworkActionAsync

---

## סימולטור

### ✓ עדכון רשימה בזמן אמת עם Async (3 נק')

רשימות מתעדכנות בזמן אמת כשהסימולטור משנה נתונים, עם Dispatcher ו-Mutex לניהול עומס.

**מימושים:**
- [`BL/Helpers/CourierManager.cs` - SimulateCourierActivityAsync](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/BL/Helpers/CourierManager.cs#L536-L755)
  - עדכון סטטוס הזמנות בזמן אמת
  - עדכון הערכות זמנים עם variance
  - התראות observers אחרי כל שינוי
- [`PL/Courier/CourierListWindow.xaml.cs` - courierListObserver](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/Courier/CourierListWindow.xaml.cs#L212-L230)
  - `ObserverMutex` למניעת flooding של עדכונים
  - `Dispatcher.BeginInvoke()` לעדכון UI בחוט הראשי
- [`PL/Helpers/ObserverMutex.cs`](https://github.com/ori-levental/dotNet5786_9587_3771/blob/main/PL/Helpers/ObserverMutex. cs#L10-L45)
  - `CheckAndSetLoadInProgressOrRestartRequired()` - ניהול תור חכם
  - `UnsetLoadInProgressAndCheckRestartRequested()` - בדיקת אם צריך להריץ שוב

---
