# دليل الموردين

تطبيق ASP.NET Core 8 واحد يجمع لوحة إدارة عربية RTL وواجهة REST API. لا يحتاج إلى Docker أو أي خدمة خارجية؛ الملفات تذهب افتراضياً إلى `wwwroot/uploads` وقاعدة البيانات SQL Server.

## التشغيل المحلي

1. عدّل `ConnectionStrings__DefaultConnection` أو `appsettings.json`.
2. عيّن `Jwt__Key` إلى مفتاح عشوائي طويل. يُنشأ المدير الافتراضي تلقائيًا عند أول تشغيل: `admin@admin.com` / `Admin@123456`.
3. شغّل `dotnet ef database update` ثم `dotnet run` من مجلد المشروع.
4. افتح `/login` للوحة الإدارة، و`/swagger` لتجربة الواجهة. استخدم `POST /api/auth/login` للحصول على JWT لإدارة `/api/admin/*`.

## النشر على SmarterASP.NET

1. نفّذ `dotnet publish -c Release -o publish` ثم ارفع محتويات `publish` إلى الموقع.
2. أنشئ قاعدة SQL Server واضبط connection string وJWT ومعلومات المدير كمتغيرات بيئة (أو إعدادات الاستضافة الآمنة).
3. امنح هوية IIS صلاحية كتابة لمجلدي `wwwroot/uploads` و`logs`.
4. نفّذ migration من جهاز التطوير/لوحة الاستضافة (`dotnet ef database update`)، أو اسمح للتطبيق بتطبيق migrations عند أول تشغيل.

## ملاحظات تصميم

* الحذف للموردين والمناطق والتصنيفات والإعلانات حذف منطقي، لذلك لا تظهر في API العامة.
* تخزين الصور وراء `IFileStorageService`؛ استبدال `LocalFileStorageService` يتيح S3/R2/Cloudinary بلا تغيير أعمال الموردين.
* يعالج البحث والتصفية والترقيم داخل استعلامات EF Core قبل `Skip/Take`.
