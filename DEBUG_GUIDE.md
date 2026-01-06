# راهنمای دیباگ مشکل Concurrency

## مشکل
خطای `DbUpdateConcurrencyException` هنگام ذخیره محصول با تصاویر:
```
Failed to update product. The database operation was expected to affect 1 row(s), but actually affected 0 row(s)
```

## اطلاعات مورد نیاز برای دیباگ

### 1. لاگ‌های Debug (System.Diagnostics.Debug)

**چطور ببینیم:**
- **Visual Studio**: View → Output → در dropdown "Show output from" گزینه "Debug" را انتخاب کنید
- **VS Code**: Terminal → Output → "Debug Console" را انتخاب کنید
- **Console App**: لاگ‌ها در console نمایش داده می‌شوند

**لاگ‌های مهم:**
- `[UpdateProductCommand]` - لاگ‌های مربوط به command handler
- `[RowVersion Debug]` - لاگ‌های مربوط به RowVersion در interceptor
- `[Update Debug]` - لاگ‌های مربوط به repository Update method

**چه چیزی را کپی کنیم:**
تمام لاگ‌هایی که با این prefix شروع می‌شوند، از لحظه کلیک روی Save تا خطا

### 2. لاگ‌های SQL (EF Core)

**چطور ببینیم:**
لاگ‌های SQL در console/terminal که پروژه را run کرده‌اید نمایش داده می‌شوند.

**چه چیزی را کپی کنیم:**
- SQL query که برای UPDATE اجرا می‌شود
- به خصوص قسمت WHERE که شامل `RowVersion` است
- خطاهای SQL (اگر وجود دارد)

**مثال:**
```
Executed DbCommand (Xms) [Parameters=[@p0='...', @p1='...'], CommandType='Text', CommandTimeout='30']
UPDATE "Products" SET ... WHERE "Id" = @p0 AND "RowVersion" = @p1
```

### 3. اطلاعات Product در دیتابیس

اگر به دیتابیس دسترسی دارید، این query را اجرا کنید:

```sql
SELECT 
    "Id",
    "Name",
    encode("RowVersion", 'hex') as row_version_hex,
    LENGTH("RowVersion") as row_version_length
FROM "Products"
WHERE "Id" = 'YOUR_PRODUCT_ID';
```

**چه چیزی را بفرستید:**
- `row_version_hex`: مقدار RowVersion به صورت hex
- `row_version_length`: طول RowVersion (باید 24 باشد)

### 4. مراحل تست

1. پروژه را run کنید
2. به `/admin/products` بروید
3. یک محصول را edit کنید
4. یک عکس اضافه یا حذف کنید
5. روی Save کلیک کنید
6. **فوراً** تمام لاگ‌ها را کپی کنید (قبل از اینکه صفحه refresh شود)

### 5. اطلاعات اضافی مفید

- **Product ID**: ID محصولی که edit می‌کنید
- **تعداد تصاویر**: قبل و بعد از edit
- **زمان دقیق**: چه زمانی خطا رخ داد

## مثال خروجی مورد نیاز

```
[UpdateProductCommand] Initial Product RowVersion: 0123456789ABCDEF...
[RowVersion Debug] Entity: Product, Id: xxx
[RowVersion Debug] Original RowVersion: 0123456789ABCDEF...
[RowVersion Debug] New RowVersion: FEDCBA9876543210...
[UpdateProductCommand] Before SaveChanges - Product RowVersion: FEDCBA9876543210...
Executed DbCommand (5ms) [Parameters=[@p0='xxx', @p1='0123456789ABCDEF...'], CommandType='Text', CommandTimeout='30']
UPDATE "Products" SET ... WHERE "Id" = @p0 AND "RowVersion" = @p1
[UpdateProductCommand] Concurrency exception caught: ...
```

## نکات مهم

1. **همه لاگ‌ها را کپی کنید** - حتی اگر به نظر بی‌ربط می‌رسند
2. **SQL query کامل** - به خصوص قسمت WHERE
3. **RowVersion hex values** - برای مقایسه
4. **خطاهای کامل** - با stack trace

## اگر MCP Postgres کار می‌کند

می‌توانم مستقیماً دیتابیس را بررسی کنم. فقط بگویید:
- Connection string چیست؟
- Product ID چیست؟

