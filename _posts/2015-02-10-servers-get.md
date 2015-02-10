---
category: 'Servers'
path: '/servers'
title: 'Get servers'
type: 'GET'

layout: null
---

This method allows users to see a list of all servers managed by this API.

### Request

* The headers must include a **[valid authentication token](#authentication)**.

### Response

Sends back a collection of servers managed by this API.

```Status: 200 OK```
```{
    {
        id: 'win1',
        dns: 'win1.exira.com',
        description: 'Windows 2012 R2 @ Frankfurt'
    },
    {
        id: 'win2',
        dns: 'win2.exira.com',
        description: 'Windows 2008 @ Dublin'
    }
}```

For errors responses, see the [response status codes documentation](#response-status-codes).
