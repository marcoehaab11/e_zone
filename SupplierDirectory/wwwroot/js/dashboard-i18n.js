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
    'الصورة': 'Image', 'الحالة': 'Status', 'الإجراءات': 'Actions', 'الاسم': 'Name', 'الموردون': 'Suppliers', 'المناطق': 'Areas', 'التصنيفات': 'Categories'
  }));

  const translate = value => words.get(value.trim()) || value;
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
})();
