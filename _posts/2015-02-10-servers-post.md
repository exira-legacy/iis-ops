---
category: 'Servers'
path: '/servers'
title: 'Add a server'
type: 'POST'
order: 2

layout: null
---

This method allows to add a new server to be managed.

### Request

* The headers must include a **[valid authentication token](#authentication)**.

```Authentication: bearer TOKEN```
```{
    slug: 'win3',
    dns: 'win3.exira.com',
    description: 'Windows 2012 R2 @ Oregon'
}```

### Response

**If succeeds**, returns the managed server.

```Status: 201 Created```
```{
    slug: 'win3',
    dns: 'win3.exira.com',
    description: 'Windows 2012 R2 @ Oregon'
}```

For errors responses, see the [response status codes documentation](#response-status-codes).
