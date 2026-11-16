(() => {
  if (document.documentElement.lang !== 'en') return;

  const words = new Map(Object.entries({
    'الرئيسية': 'Dashboard', 'إدارة الموردين': 'Suppliers', 'قائمة الموردين': 'Supplier list', 'إضافة مورد جديد': 'Add supplier',
    'إدارة المناطق': 'Areas', 'المناطق المتاحة': 'Available areas', 'إضافة منطقة': 'Add area',
    'تصنيفات الموردين': 'Supplier categories', 'قائمة تصنيفات الموردين': 'Supplier category list', 'إضافة تصنيف مورد': 'Add supplier category',
    'تصنيفات المنتجات': 'Product categories', 'قائمة تصنيفات المنتجات': 'Product category list', 'إضافة تصنيف منتج': 'Add product category',
    'إدارة المنتجات': 'Products', 'قائمة المنتجات': 'Product list', 'إضافة منتج': 'Add product',
    'إدارة خدماتنا': 'Our services', 'قائمة الخدمات': 'Service list', 'إضافة خدمة': 'Add service', 'تصنيفات الخدمات': 'Service categories', 'إضافة تصنيف خدمة': 'Add service category',
    'إدارة الإعلانات': 'Advertisements', 'الإعلانات': 'Advertisements', 'إضافة إعلان': 'Add advertisement',
    'معلومات المنصة': 'Platform information', 'إدارة المستخدمين': 'User management', 'تسجيل الخروج': 'Sign out', 'خروج': 'Sign out',
    'رجوع': 'Back', 'رجوع للقائمة': 'Back to list', 'إلغاء': 'Cancel', 'بحث': 'Search', 'حفظ البيانات': 'Save changes', 'حفظ الخدمة': 'Save service', 'حفظ المنتج': 'Save product', 'حفظ التصنيف': 'Save category',
    'تعديل': 'Edit', 'حذف': 'Delete', 'تفعيل': 'Activate', 'تعطيل': 'Disable', 'نشط': 'Active', 'معطل': 'Disabled',
    'اسم المورد': 'Supplier name', 'الوصف': 'Description', 'العنوان التفصيلي': 'Full address', 'العنوان المختصر (يظهر في القوائم)': 'Short address (shown in lists)',
    'اسم المنطقة': 'Area name', 'المنطقة الأم': 'Parent area', 'اسم التصنيف': 'Category name', 'التصنيف الرئيسي (الأب)': 'Parent category',
    'اسم المنتج': 'Product name', 'تصنيف المنتج': 'Product category', 'المنطقة (اختياري)': 'Area (optional)', 'وصف قصير': 'Short description', 'تفاصيل ومعلومات إضافية': 'Details',
    'اسم الخدمة': 'Service name', 'تصنيف الخدمة': 'Service category', 'ترتيب العرض': 'Display order', 'تفاصيل وشرح موسع للخدمة': 'Service details',
    'عنوان الإعلان': 'Advertisement title', 'رابط التوجيه (Link)': 'Destination link', 'تاريخ بدء العرض': 'Start date', 'تاريخ انتهاء العرض': 'End date',
    'اسم الشركة / المنصة': 'Company / platform name', 'عن الشركة': 'About', 'رسالتنا (Mission)': 'Mission', 'رؤيتنا (Vision)': 'Vision', 'وصف المنصة': 'Platform description', 'خدمات المنصة': 'Platform services',
    'كل المناطق (إعلان عام)': 'All areas (general advertisement)', 'عام (متوفر في كل المناطق)': 'General (available in all areas)', '(بدون تصنيف)': '(No category)',
    'الصورة': 'Image', 'صورة': 'Image', 'الشعار': 'Logo', 'الحالة': 'Status', 'الإجراءات': 'Actions', 'إجراءات': 'Actions', 'الاسم': 'Name', 'الموردون': 'Suppliers', 'المناطق': 'Areas', 'التصنيفات': 'Categories',

    // Dashboard overview
    'لوحة التحكم والإحصائيات': 'Dashboard overview', 'إجمالي الموردين': 'Total suppliers', 'الموردون النشطون': 'Active suppliers',
    'إجمالي المناطق': 'Total areas', 'إجمالي التصنيفات': 'Total categories', 'إجمالي الإعلانات': 'Total advertisements',
    'الإعلانات النشطة': 'Active advertisements', 'أحدث الموردين المضافين': 'Recently added suppliers', 'تاريخ الإضافة': 'Date added',
    'عرض كل الموردين': 'View all suppliers', 'لا يوجد موردين مضافين حديثاً.': 'No recently added suppliers.',

    // Shared list controls, columns and states
    'كل التصنيفات': 'All categories', 'كل المناطق': 'All areas', 'كل المستويات': 'All levels', 'تصفية': 'Filter', 'إعادة تعيين': 'Reset',
    'الهاتف': 'Phone', 'البريد الإلكتروني': 'Email address', 'المنطقة': 'Area', 'العنوان': 'Title', 'الترتيب': 'Order',
    'تاريخ البدء': 'Start date', 'تاريخ الانتهاء': 'End date', 'المستوى': 'Level', 'المنتجات': 'Products',
    'غير نشط': 'Inactive', 'موقوف': 'Stopped', 'منتهي': 'Expired', 'مجدول': 'Scheduled', 'بدون تصنيف': 'No category',
    'تصنيف رئيسي': 'Main category', 'التصنيفات الرئيسية فقط': 'Main categories only', 'فرعي من:': 'Child of:', 'تحت:': 'Under:',
    'أضيف في:': 'Added on:', 'منتج': 'product', 'خدمة': 'service', 'صورة': 'image', 'صور': 'images',
    'لا توجد نتائج مطابقة للبحث': 'No matching results found.', 'لا يوجد منتجات مطابقة للبحث': 'No matching products found.',
    'لا توجد تصنيفات منتجات مطابقة للبحث': 'No matching product categories found.', 'لا يوجد موردين متاحين للبحث الحالي': 'No suppliers match the current search.',
    'لا توجد إعلانات لعرضها': 'No advertisements to display.', 'لا توجد تصنيفات خدمات حتى الآن': 'No service categories yet.',
    'لا يوجد مستخدمين لعرضهم': 'No users to display.',

    // Suppliers
    'إضافة مورد': 'Add supplier', 'بحث باسم المورد أو رقم الهاتف...': 'Search by supplier name or phone number...',
    'رقم الهاتف الأساسي': 'Primary phone number', 'رقم الواتساب': 'WhatsApp number', 'أرقام إضافية': 'Additional phone numbers',
    'الموقع الإلكتروني': 'Website', 'بيانات التواصل': 'Contact details', 'الموقع الجغرافي': 'Location',
    'الموقع الجغرافي (الفرع الأساسي)': 'Location (main branch)', 'التصنيفات والمناطق': 'Categories and areas',
    'هل المورد يوفر فنيين؟ (عمالة وصيانة)': 'Does this supplier provide technicians?', 'المورد نشط ويمكن ظهوره في الموقع': 'Supplier is active and visible on the site',
    'البيانات الأساسية': 'Basic information', 'معرض الصور': 'Image gallery', 'رفع صور جديدة': 'Upload new images',
    'هل أنت متأكد من نقل المورد إلى سلة المحذوفات؟': 'Are you sure you want to move this supplier to the recycle bin?',

    // Areas and categories
    'إدارة تصنيفات المنتجات': 'Product category management', 'تنظيم وهيكلة تصنيفات وأقسام المنتجات الرئيسية والفرعية': 'Organize main and sub-product categories.',
    'بحث باسم تصنيف المنتج أو الوصف...': 'Search by product category name or description...', '-- كل المستويات والتصنيفات --': '-- All levels and categories --',
    'اسم التصنيف': 'Category name', 'التصنيف الأب': 'Parent category', 'الوصف': 'Description', 'الوصف المختصر': 'Short description',
    'لا يمكن حذف تصنيف يحتوي على تصنيفات فرعية أو خدمات': 'A category with child categories or services cannot be deleted.',
    'هل أنت متأكد من حذف هذا التصنيف؟': 'Are you sure you want to delete this category?', 'هل تريد حذف التصنيف؟': 'Do you want to delete this category?',
    'إضافة تصنيف جديد': 'Add category', 'إضافة تصنيف': 'Add category', 'إضافة تصنيف خدمة': 'Add service category',
    'بحث باسم أو وصف التصنيف': 'Search by category name or description', 'التصنيف الأب': 'Parent category',
    'إدارة تصنيفات الموردين': 'Supplier category management', 'تنظيم وهيكلة مجالات وتصنيفات الموردين الرئيسية والفرعية': 'Organize main and sub-supplier categories.',
    'بحث باسم التصنيف أو الوصف...': 'Search by category name or description...', 'اسم تصنيف المنتج': 'Product category name',
    'تعديل تصنيف المورد': 'Edit supplier category', 'إضافة تصنيف مورد جديد': 'Add supplier category',
    'تحديث بيانات وصورة ومستوى التصنيف': 'Update category details, image, and level', 'إنشاء تصنيف رئيسي أو فرعي جديد لموردي المنصة': 'Create a new main or subcategory for platform suppliers.',
    'تعديل تصنيف المنتج': 'Edit product category', 'إضافة تصنيف منتج جديد': 'Add product category',
    'تحديث بيانات وصورة ومستوى تصنيف المنتج': 'Update product category details, image, and level', 'إنشاء تصنيف رئيسي أو فرعي جديد للمنتجات': 'Create a new main or subcategory for products.',
    'أدخل اسم التصنيف (مثال: أجهزة إلكترونية، مقاولات، ملابس)...': 'Enter a category name (for example: electronics, contracting, clothing)...',
    'أدخل اسم التصنيف (مثال: هواتف ذكية، أدوات كهربائية، قطع غيار)...': 'Enter a category name (for example: smartphones, electrical tools, spare parts)...',
    'التصنيف الرئيسي (الأب)': 'Parent category', '(مستوى رئيسي - بدون تصنيف أم)': '(Main level — no parent category)',
    'اتركه فارغاً إذا كان هذا تصنيفاً رئيسياً، أو اختر التصنيف الرئيسي الذي يندرج تحته.': 'Leave empty for a main category, or choose the parent category.',
    'نبذة توضيحية عن مجالات ونشاط هذا التصنيف (اختياري)...': 'A brief description of this category and its activity (optional)...',
    'نبذة توضيحية عن المنتجات التي تندرج تحت هذا التصنيف (اختياري)...': 'A brief description of products in this category (optional)...',
    'صورة / أيقونة التصنيف': 'Category image / icon', 'الصيغ المدعومة: JPG, PNG, WEBP (الحجم الأقصى: 5 ميجابايت).': 'Supported formats: JPG, PNG, WEBP (maximum 5 MB).',
    'معاينة الصورة:': 'Image preview:', 'التصنيف نشط ويظهر في المنصة': 'Category is active and visible on the platform',

    // Products
    'قائمة المنتجات': 'Product list', 'إدارة وعرض المنتجات وتصنيفاتها والمناطق التابعة لها': 'Manage products, their categories, and their areas.',
    'إضافة منتج جديد': 'Add product', 'ابحث بالاسم أو الوصف...': 'Search by name or description...', 'كل تصنيفات المنتجات': 'All product categories',
    'اسم المنتج': 'Product name', 'المنطقة (اختياري)': 'Area (optional)', 'عام (متوفر في كل المناطق)': 'General (available in all areas)',
    'المنتج نشط ومتاح للعرض': 'Product is active and available to display', 'صور المنتج': 'Product images',
    'رفع صور جديدة (اختياري - يمكنك اختيار أكثر من صورة)': 'Upload new images (optional — you can choose more than one image)',
    'الصور الحالية:': 'Current images:', 'حفظ المنتج': 'Save product', 'هل أنت متأكد من حذف هذا المنتج؟': 'Are you sure you want to delete this product?',
    'هل أنت متأكد من حذف هذه الصورة؟': 'Are you sure you want to delete this image?', 'حدث خطأ': 'An error occurred', 'حدث خطأ أثناء حذف الصورة': 'An error occurred while deleting the image',
    'إدخال تفاصيل المنتج وتصنيفه والمنطقة والصور': 'Enter product details, category, area, and images.',
    'أدخل اسم المنتج...': 'Enter the product name...', 'وصف موجز للمنتج...': 'A short product description...',
    'المواصفات الفنية أو التفاصيل الكاملة...': 'Technical specifications or full details...',

    // Services
    'إدارة قسم خدماتنا': 'Service management', 'إضافة وتعديل الخدمات التي تقدمها المنصة وصورها وتفاصيلها': 'Add and manage platform services, images, and details.',
    'إضافة خدمة جديدة': 'Add service', 'ابحث باسم الخدمة أو الوصف...': 'Search by service name or description...',
    'اسم الخدمة': 'Service name', 'عدد الصور': 'Image count', 'أضيفت في:': 'Added on:', 'الخدمة نشطة وتظهر في المنصة': 'Service is active and visible on the platform',
    'صور الخدمة': 'Service images', 'رفع صور جديدة (اختياري - يمكنك اختيار صورة واحدة أو أكثر)': 'Upload new images (optional — choose one or more images)',
    'الصيغ المدعومة: JPG, PNG, WEBP (الحجم الأقصى لكل صورة: 5 ميجابايت).': 'Supported formats: JPG, PNG, WEBP (maximum 5 MB per image).',
    'الصور الحالية للخدمة:': 'Current service images:', 'هل أنت متأكد من حذف هذه الخدمة؟': 'Are you sure you want to delete this service?',
    'تعديل الخدمة': 'Edit service', 'تحديث بيانات وصور وتفاصيل الخدمة': 'Update service details and images',
    'إدخال خدمة جديدة وعرضها في تطبيق وموقع المنصة': 'Add a new service for the platform app and website.',
    'أدخل اسم الخدمة (مثال: صيانة أجهزة، استشارات فنية، شحن وتوصيل)...': 'Enter the service name (for example: maintenance, technical consultation, delivery)...',
    'وصف موجز للخدمة يظهر في القوائم والبطاقات...': 'A short service description shown in lists and cards...',
    'شرح مفصل عن كيفية تقديم الخدمة والمزايا والتفاصيل الفنية (اختياري)...': 'A detailed explanation of the service, benefits, and technical details (optional)...',
    'تصنيفات خدماتنا': 'Service categories', 'تصنيفات مستقلة لتنظيم الخدمات المعروضة في المنصة.': 'Independent categories for organizing services shown on the platform.',
    'التصنيف': 'Category', 'الفرعي': 'Subcategories', 'تعديل تصنيف خدمة': 'Edit service category',
    'مثال: خدمات الصيانة': 'Example: Maintenance services', 'وصف مختصر للتصنيف': 'Short category description',
    'التصنيف نشط ويظهر في الاختيارات': 'Category is active and available for selection',

    // Advertisements
    'إضافة إعلان': 'Add advertisement', 'بحث بعنوان الإعلان أو الوصف...': 'Search by advertisement title or description...',
    'صورة الإعلان': 'Advertisement image', 'صورة الإعلان (بنر)': 'Advertisement image (banner)', 'رابط الإعلان': 'Advertisement link',
    'عنوان الإعلان': 'Advertisement title', 'أدخل عنوان أو اسم للإعلان...': 'Enter an advertisement title or name...',
    'وصف إضافي للإعلان (اختياري)': 'Additional advertisement description (optional)', 'الإعلان نشط': 'Advertisement is active',
    'هل أنت متأكد من حذف الإعلان؟': 'Are you sure you want to delete this advertisement?',

    // Company and users
    'معلومات الشركة': 'Company information', 'معلومات الشركة والمنصة': 'Company and platform information',
    'شعار الشركة (Logo)': 'Company logo', 'من نحن ورؤية المنصة': 'About us and platform vision',
    'الروابط العامة والإضافية': 'Public and additional links', 'إضافة رابط جديد': 'Add new link', 'حذف الرابط': 'Delete link',
    'لا توجد روابط إضافية مضافة حالياً. اضغط على زر "إضافة رابط جديد" لإضافة روابط للمنصة.': 'No additional links have been added. Select "Add new link" to add platform links.',
    'صور المنصة (معرض الصور)': 'Platform images', 'رفع صور جديدة للمنصة (اختياري - يمكنك اختيار أكثر من صورة)': 'Upload new platform images (optional — choose more than one image)',
    'حفظ المعلومات': 'Save information', 'إدارة المستخدمين': 'User management', 'إضافة مستخدم': 'Add user',
    'كلمة المرور': 'Password', 'تأكيد كلمة المرور': 'Confirm password', 'أدخل كلمة المرور': 'Enter password', 'أعد إدخال كلمة المرور': 'Re-enter password',
    'هل أنت متأكد من حذف هذا المستخدم؟': 'Are you sure you want to delete this user?'
  }));

  const translate = value => words.get(value.trim()) || value;
  const nativeConfirm = window.confirm.bind(window);
  const nativeAlert = window.alert.bind(window);
  window.confirm = message => nativeConfirm(translate(message));
  window.alert = message => nativeAlert(translate(message));
  const walk = root => {
    const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
    const nodes = [];
    while (walker.nextNode()) nodes.push(walker.currentNode);
    nodes.forEach(node => {
      const original = node.nodeValue;
      const trim = original.trim();
      if (!trim || !words.has(trim)) return;
      node.nodeValue = original.replace(trim, words.get(trim));
    });
  };
  walk(document.body);
  document.querySelectorAll('[placeholder],[title],[aria-label]').forEach(element => {
    ['placeholder', 'title', 'aria-label'].forEach(attribute => {
      const value = element.getAttribute(attribute);
      if (value) element.setAttribute(attribute, translate(value));
    });
  });
  document.title = translate(document.title.replace(/\s+-\s+Power Zone$/, '')) + ' - Power Zone';
})();
