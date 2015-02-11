---
category: 'Servers'
path: '/servers/{id}'
title: 'Get server'
type: 'GET'
order: 3

layout: null
---

This method allows to see all details of a managed server.

### Request

```GET /servers/a1b2c3```

* The headers must include a **[valid authentication token](#authentication)**.

### Response

Sends back all details of a managed server.

```Status: 200 OK```
```{
    "meta": {
        "href": "https://api/servers/a1b2c3"
    },
    "dns": "win1.exira.com",
    "description": "Windows 2012 R2 @ Frankfurt",
    ...
}```

For errors responses, see the [response status codes documentation](#response-status-codes).
