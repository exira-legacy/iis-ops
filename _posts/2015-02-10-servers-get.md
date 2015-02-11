---
category: 'Servers'
path: '/servers'
title: 'Get servers'
type: 'GET'
order: 1

layout: null
---

This method allows to see a list of all managed servers.

### Request

```GET /servers```

* The headers must include a **[valid authentication token](#authentication)**.

### Response

Sends back a collection of managed servers.

```Status: 200 OK```
```{
    {
        "meta": {
            "href": "https://api/servers/a1b2c3"
        },
        "dns": "win1.exira.com",
        "description": "Windows 2012 R2 @ Frankfurt",
    },
    {
        "meta": {
            "href": "https://api/servers/b5c3a1"
        },
        "dns": "win2.exira.com",
        "description": "Windows 2008 @ Dublin"
    }
}```

For errors responses, see the [response status codes documentation](#response-status-codes).
