# دليل استهلاك واجهة برمجة التطبيقات (Mobile API Documentation) - E-Zone

يحتوي هذا الملف على جميع مسارات (Endpoints) الـ `GET` المخصصة لتطبيق الموبايل لعرض البيانات للزوار والمستخدمين.

**الرابط الأساسي (Base URL):** `http://ezone.runasp.net/api` (أو `https://localhost:57602/api` محلياً)

**الهيدر المطلوب (Header Authentication):**
يجب إرسال الـ API Key التالي في الـ Headers مع كل طلب:
```http
MobileApiKey: EZone-Mobile-App-Key-8XyZ2pL9mQ4vR1wT
```

**هيكل الرد الثابت (Base Response):** جميع الردود تأتي بـ 3 حقول رئيسية:
```json
{
  "success": true,   // أو false في حالة الخطأ
  "message": "...",  // رسالة نصية (مثل: تم استرجاع البيانات)
  "data": { ... }    // البيانات المطلوبة أو null
}
```
عند وجود صفحات (Pagination)، يتم إرجاع الـ Data بداخلها هيكل كالتالي:
```json
"data": {
    "items": [ ... ], // المصفوفة
    "meta": { "currentPage": 1, "pageSize": 20, "totalPages": 5, "totalCount": 100, "hasNext": true, "hasPrevious": false }
}
```

---

## 1. معلومات الشركة والمنصة (Company Info)
- **الوصف:** جلب معلومات المنصة والتواصل، الروابط العامة، ومعرض الصور لعرضها في صفحة "عن التطبيق" أو "اتصل بنا".
- **المسار:** `GET /api/company`
- **Response (مثال):**
```json
{
  "success": true,
  "message": "تم الاسترجاع",
  "data": {
    "id": 1,
    "companyName": "E-Zone",
    "logoUrl": "/uploads/company/logo.png",
    "coverImageUrl": "/uploads/company/cover.jpg",
    "about": "نبذة عن المنصة...",
    "mission": "رسالتنا...",
    "vision": "رؤيتنا...",
    "platformDescription": "وصف المنصة",
    "platformServices": "خدماتنا...",
    "contactPhone": "01000000000",
    "whatsApp": "01000000000",
    "email": "info@example.com",
    "website": "https://example.com",
    "socialLinksJson": null,
    "links": [
      {
        "id": 1,
        "title": "فيسبوك",
        "url": "https://facebook.com/example",
        "displayOrder": 1
      },
      {
        "id": 2,
        "title": "سياسة الاستخدام والخصوصية",
        "url": "https://example.com/terms",
        "displayOrder": 2
      }
    ],
    "images": [
      {
        "id": 1,
        "imageUrl": "/uploads/company/images/img1.jpg",
        "displayOrder": 1
      },
      {
        "id": 2,
        "imageUrl": "/uploads/company/images/img2.jpg",
        "displayOrder": 2
      }
    ]
  }
}
```

---

## 2. الإعلانات والبنرات (Advertisements)
- **الوصف:** جلب قائمة البنرات الإعلانية النشطة لعرضها في سلايدر الصفحة الرئيسية مع إمكانية الفلترة بالمنطقة.
- **المسار:** `GET /api/advertisements`
- **Request Parameters (Query):**
  - `areaId` (اختياري): رقم المنطقة لعرض إعلانات مخصصة لتلك المنطقة.
- **Response (مثال):**
```json
{
  "success": true,
  "message": "تم الاسترجاع",
  "data": [
    {
      "id": 1,
      "title": "إعلان عروض خاصة",
      "description": "خصومات حصرية",
      "imageUrl": "/uploads/advertisements/image.jpg",
      "link": "https://example.com/offer",
      "displayOrder": 1,
      "areaId": 2,
      "areaName": "القاهرة"
    }
  ]
}
```

---

## 3. تصنيفات الموردين (Supplier Categories)
- **الوصف:** جلب قائمة تصنيفات الموردين (رئيسية أو فرعية) مع الصور والترقيم.
- **المسار:** `GET /api/categories` أو `GET /api/supplier-categories`
- **Request Parameters (Query):**
  - `parentId` (اختياري): 
    - مرر `0` لجلب **التصنيفات الرئيسية فقط**.
    - مرر رقم التصنيف الأب لجلب **التصنيفات الفرعية التابعة له**.
    - اتركه فارغاً لجلب كل التصنيفات.
  - `search` (اختياري): بحث بالاسم أو الوصف.
  - `page` (اختياري - Default: 1)
  - `pageSize` (اختياري - Default: 20)
- **Response (مثال):**
```json
{
  "success": true,
  "message": "تم استرجاع البيانات",
  "data": {
    "items": [
      {
        "id": 1,
        "name": "أجهزة إلكترونية",
        "description": "جميع أنواع الإلكترونيات",
        "imageUrl": "/uploads/categories/image.png",
        "parentCategoryId": null,
        "parentCategoryName": null,
        "subCategoriesCount": 4
      }
    ],
    "meta": {
      "currentPage": 1,
      "pageSize": 20,
      "totalPages": 1,
      "totalCount": 1
    }
  }
}
```

### تفاصيل تصنيف مورد محدد مع فروعه:
- **المسار:** `GET /api/categories/{id}`
- **Response (مثال):**
```json
{
  "success": true,
  "message": "تم الاسترجاع",
  "data": {
    "id": 1,
    "name": "أجهزة إلكترونية",
    "description": "...",
    "imageUrl": "/uploads/categories/image.png",
    "parentCategoryId": null,
    "parentCategoryName": null,
    "children": [
      {
        "id": 5,
        "name": "كاميرات مراقبة",
        "description": "...",
        "imageUrl": "/uploads/categories/cams.png"
      }
    ]
  }
}
```

---

## 4. تصنيفات المنتجات (Product Categories)
- **الوصف:** جلب قائمة تصنيفات المنتجات مع الهيكل الهرمي (رئيسي وفرعي).
- **المسار:** `GET /api/product-categories`
- **Request Parameters (Query):**
  - `parentId` (اختياري): `0` للتصنيفات الرئيسية فقط، أو معرف التصنيف الأب.
  - `search` (اختياري): نص البحث.
  - `page` (اختياري - Default: 1)
  - `pageSize` (اختياري - Default: 20)
- **Response (مثال):**
```json
{
  "success": true,
  "message": "تم استرجاع البيانات",
  "data": {
    "items": [
      {
        "id": 10,
        "name": "أدوات كهربائية",
        "description": "تصنيف الأدوات والمعدات",
        "imageUrl": "/uploads/product-categories/tools.png",
        "parentCategoryId": null,
        "parentCategoryName": null,
        "subCategoriesCount": 2
      }
    ],
    "meta": {
      "currentPage": 1,
      "pageSize": 20,
      "totalPages": 1,
      "totalCount": 1
    }
  }
}
```

### تفاصيل تصنيف منتج محدد مع فروعه:
- **المسار:** `GET /api/product-categories/{id}`

---

## 5. المنتجات (Products)
- **الوصف:** جلب قائمة المنتجات مع دعم البحث والتصفية حسب تصنيف المنتج أو المنطقة.
- **المسار:** `GET /api/products`
- **Request Parameters (Query):**
  - `search` (اختياري): بحث باسم أو وصف المنتج.
  - `productCategoryId` (اختياري): رقم تصنيف المنتج (يجلب منتجات التصنيف وفروعه).
  - `areaId` (اختياري): رقم المنطقة.
  - `page` (Default: 1), `pageSize` (Default: 20)
- **Response (مثال):**
```json
{
  "success": true,
  "message": "تم استرجاع البيانات",
  "data": {
    "items": [
      {
        "id": 101,
        "name": "جهاز إنذار ذكي",
        "description": "وصف قصير للمنتج",
        "details": "المواصفات الفنية الكاملة...",
        "areaId": 1,
        "areaName": "القاهرة",
        "productCategoryId": 10,
        "productCategoryName": "أدوات كهربائية",
        "images": [
          { "id": 1, "imageUrl": "/uploads/products/images/img1.jpg", "displayOrder": 1 }
        ]
      }
    ],
    "meta": {
      "currentPage": 1,
      "pageSize": 20,
      "totalPages": 1,
      "totalCount": 1
    }
  }
}
```

### تفاصيل منتج محدد:
- **المسار:** `GET /api/products/{id}`

---

## 6. قسم خدماتنا (Services)
- **الوصف:** جلب قائمة الخدمات التي تقدمها المنصة مع معرض الصور الكامل وترتيب العرض.
- **المسار:** `GET /api/services`
- **Request Parameters (Query):**
  - `search` (اختياري): نص البحث في اسم أو وصف الخدمة.
  - `page` (Default: 1), `pageSize` (Default: 20)
- **Response (مثال):**
```json
{
  "success": true,
  "message": "تم استرجاع البيانات",
  "data": {
    "items": [
      {
        "id": 1,
        "name": "خدمة التركيب والصيانة الفنية",
        "description": "صيانة دورية وتركيب لجميع الأجهزة",
        "details": "تفاصيل ومراحل تقديم الخدمة والمعدات المستخدمة...",
        "displayOrder": 1,
        "images": [
          { "id": 1, "imageUrl": "/uploads/services/images/service1.jpg", "displayOrder": 1 },
          { "id": 2, "imageUrl": "/uploads/services/images/service2.jpg", "displayOrder": 2 }
        ]
      }
    ],
    "meta": {
      "currentPage": 1,
      "pageSize": 20,
      "totalPages": 1,
      "totalCount": 1
    }
  }
}
```

### تفاصيل خدمة محددة:
- **المسار:** `GET /api/services/{id}`

---

## 7. المناطق والمدن (Areas)
- **المسار:** `GET /api/areas`
- **Request Parameters (Query):**
  - `parentId` (اختياري): `0` للمناطق الرئيسية فقط، أو معرف المحافظة/المنطقة لجلب أحيائها.
  - `search` (اختياري): نص البحث.
  - `page` (Default: 1), `pageSize` (Default: 20)

---

## 8. قائمة الموردين (Suppliers List)
- **المسار:** `GET /api/suppliers`
- **Request Parameters (Query):**
  - `search` (اختياري): بحث بالاسم أو الوصف.
  - `categoryId` (اختياري): رقم تصنيف المورد.
  - `areaId` (اختياري): رقم المنطقة.
  - `sort` (اختياري): `name_desc` للترتيب التنازلي.
  - `page` (Default: 1), `pageSize` (Default: 20)

### تفاصيل مورد محدد:
- **المسار:** `GET /api/suppliers/{id}`
