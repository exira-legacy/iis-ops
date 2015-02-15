---
category: 'Sites'
path: '/servers/{server}/sites'
title: 'Create a site'
type: 'POST'
order: 2

layout: null
---

This method allows to add a new site.

### Request

```POST /servers/a1b2c3/sites
Authentication: bearer TOKEN
{
    "name": "www.example.org"
}```

* The headers must include a **[valid authentication token](#authentication)**.

### Response

**If succeeds**, returns the site.

```Status: 201 Created
Location: https://api/servers/a1b2c3/sites/z7y8x9```
```{
    "meta": {
        "href": "https://api/servers/a1b2c3/sites/z7y8x9"
    },
    "name": "www.example.org",
    "path": "D:\Data\www.example.org",
    "state": "Started",
    "server": {
        "meta": {
            "href": "https://api/servers/a1b2c3"
        }
    },
}```

For errors responses, see the [response status codes documentation](#response-status-codes).
