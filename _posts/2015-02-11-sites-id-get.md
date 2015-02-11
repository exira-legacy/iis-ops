---
category: 'Sites'
path: '/servers/{server}/sites/{id}'
title: 'Get site'
type: 'GET'
order: 3

layout: null
---

This method allows to see a site.

### Request

```GET /servers/a1b2c3/sites/z1y2x3```

* The headers must include a **[valid authentication token](#authentication)**.

### Response

Sends back a site.

```Status: 200 OK```
```{
    "meta": {
        "href": "https://api/servers/a1b2c3/sites/z1y2x3"
    },
    "name": "www.exira.com",
    "path": "D:\Data\www.exira.com",
    "state": "Started",
    "server": {
        "meta": {
            "href": "https://api/servers/a1b2c3"
        }
    },
    ...
}```

For errors responses, see the [response status codes documentation](#response-status-codes).
