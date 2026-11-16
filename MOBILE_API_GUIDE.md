# Power Zone — دليل API للموبايل

## عنوان API

استخدم عنوان موقع التطبيق ثم أضف المسار المطلوب، مثال:

```text
https://your-domain.com/api/services
```

## المفتاح واللغة

كل طلب Public API يحتاج هذه الرؤوس:

```http
MobileApiKey: YOUR_MOBILE_API_KEY
Accept-Language: ar
```

للإنجليزية:

```http
Accept-Language: en
```

القيم المقبولة حاليًا هي `ar` و`en`. العربية هي القيمة الافتراضية. إذا كان النص الإنجليزي غير مُسجل بعد، ترجع API النص العربي بدلًا من قيمة فارغة.

## تصنيفات الخدمات

### كل التصنيفات

```http
GET /api/service-categories?page=1&pageSize=20
```

### التصنيفات الرئيسية فقط

```http
GET /api/service-categories?parentId=0
```

### التصنيفات الفرعية لتصنيف معيّن

```http
GET /api/service-categories?parentId=12
```

### تصنيف واحد

```http
GET /api/service-categories/12
```

البيانات تتضمن: `id`, `name`, `description`, `parentServiceCategoryId`, `parentServiceCategoryName`, `servicesCount`, و`subCategoriesCount`.

## خدماتنا

### قائمة الخدمات

```http
GET /api/services?page=1&pageSize=20
```

### فلترة حسب التصنيف

```http
GET /api/services?categoryId=12
```

### خدمة واحدة

```http
GET /api/services/35
```

تحتوي الخدمة على اسم ووصف وتفاصيل وفق اللغة المطلوبة، بالإضافة إلى `serviceCategoryId`, `serviceCategoryName` و`images`.

## مثال Flutter/Dart

```dart
final response = await http.get(
  Uri.parse('$baseUrl/api/services?categoryId=12'),
  headers: {
    'MobileApiKey': mobileApiKey,
    'Accept-Language': 'en',
  },
);
```

## استجابة ناجحة

```json
{
  "success": true,
  "message": "تم استرجاع البيانات",
  "data": [],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalCount": 0,
    "totalPages": 0,
    "hasNext": false,
    "hasPrevious": false
  }
}
```

لا تضع `MobileApiKey` داخل مستودع عام أو داخل تطبيق يمكن تفكيكه دون حماية مناسبة.
