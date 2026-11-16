# E-Zone — Frontend handoff

## URLs

- Website: `http://ezone.runasp.net/`
- Public API base URL: `http://ezone.runasp.net/api`
- Admin login: `http://ezone.runasp.net/login`

## Scope for the frontend

Build the public experience of a supplier directory that lets visitors discover suppliers, products, services, areas, categories, advertisements, and company details. The dashboard is already responsible for creating and managing this content.

The frontend is read-only for public content and must use the Public API below. Do not call Dashboard routes from the frontend.

## Required headers

Every Public API call requires:

```http
MobileApiKey: <provided separately>
Accept-Language: ar
```

For English, send `Accept-Language: en`. Arabic is the default. If an English value was not entered in the dashboard, the API safely returns the Arabic value instead.

> Do not commit the API key to a public repository. Put it in the frontend environment configuration.

## Language behavior

- Default language: Arabic, RTL.
- English: LTR.
- Store the user's selected language in frontend state.
- Send the selected language with every API request using `Accept-Language`.
- Use returned fields such as `name`, `description`, and `details` directly; the API already selects Arabic or English.
- All visible frontend UI text (buttons, empty states, labels, validation messages) must have Arabic and English translations.

## API response shape

Successful responses use this structure:

```json
{
  "success": true,
  "message": "...",
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

`pagination` is returned by list endpoints that support paging. Read the list from `data`.

## Pages and API mapping

| Frontend page / section | API endpoint | Query options |
| --- | --- | --- |
| Home | `GET /company`, `GET /advertisements`, `GET /services` | Ads: `areaId`; services: `page`, `pageSize`, `categoryId` |
| Supplier directory | `GET /suppliers` | `page`, `pageSize`, `search`, `categoryId`, `areaId`, `sort` |
| Supplier details | `GET /suppliers/{id}` | — |
| Areas | `GET /areas` | `page`, `pageSize`, `search`, `parentId` (`0` = root only) |
| Supplier categories | `GET /categories` | `page`, `pageSize`, `search`, `parentId` |
| Supplier category details | `GET /categories/{id}` | — |
| Product categories | `GET /product-categories` | `page`, `pageSize`, `search`, `parentId` |
| Products | `GET /products` | `page`, `pageSize`, `search`, `areaId`, `productCategoryId` |
| Product details | `GET /products/{id}` | — |
| Service categories | `GET /service-categories` | `page`, `pageSize`, `search`, `parentId` |
| Service details | `GET /services/{id}` | — |
| Service listing | `GET /services` | `page`, `pageSize`, `search`, `categoryId` |

All endpoint paths above are relative to the API base URL. Example: `GET http://ezone.runasp.net/api/services?page=1&pageSize=20`.

## Main content fields

- Supplier: name, description, logo, phone numbers, WhatsApp, email, website, address, map coordinates/link, categories, areas, gallery images.
- Product: name, description, details, area, product category, images.
- Service: name, description, details, display order, service category, images.
- Categories and areas: name, description, optional parent and image where available.
- Advertisement: title, description, image, optional link, ordering, optional area.
- Company: company name, logo, cover images, About, Mission, Vision, platform description/services, contact information, social links, images.

## Suggested public pages

1. Home: company hero, active advertisements, primary service categories, featured services/products, and a supplier search entry point.
2. Suppliers: filters for text, area, and supplier category; paginated cards/list.
3. Supplier details: contact CTA (phone/WhatsApp), location, categories, areas, gallery, and map.
4. Services: category filter, paginated cards, and service details.
5. Products: area/category filters, paginated cards, and product details.
6. About: company content and contact/social links.

## Acceptance checklist

- Arabic is RTL and English is LTR.
- Every request contains both `MobileApiKey` and `Accept-Language`.
- Loading, empty, error, and retry states are implemented for each API view.
- Images have meaningful alternative text and a fallback when absent.
- Pagination uses the API's `pagination` values.
- External website and advertisement links open safely in a new tab.
- The UI is responsive on phone, tablet, and desktop.

## Dashboard boundary

Content management is done in the admin dashboard only. The frontend should not attempt to create, edit, delete, or toggle suppliers, products, services, categories, areas, advertisements, users, or company data.
