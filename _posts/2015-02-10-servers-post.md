---
category: 'Servers'
path: '/servers'
title: 'Initialise a server'
type: 'POST'
order: 2

layout: null
---

This method allows to add a new server to be managed.

### Request

```POST /servers
Authentication: bearer TOKEN```
```{
    "dns": "win3.exira.com",
    "description": "Windows 2012 R2 @ Oregon"
}```

* The headers must include a **[valid authentication token](#authentication)**.

### Response

**If succeeds**, returns the managed server.

```Status: 201 Created
Location: https://api/servers/d2f4g6```
```{
    "meta": {
        "href": "https://api/servers/d2f4g6"
    },
    "dns": "win3.exira.com",
    "description": "Windows 2012 R2 @ Oregon"
}```

For errors responses, see the [response status codes documentation](#response-status-codes).
