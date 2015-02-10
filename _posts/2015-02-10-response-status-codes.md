---
title: 'Response status codes'

layout: null
---

### Success

Successes differ from errors in that their body may not be a simple response object with a code and a message. The headers however are consistent across all calls:

* `GET`, `PUT`, `DELETE` returns `200 OK` on success.
* `POST` returns `201 Created` on success.

### Error

Error responses are simply returning [standard HTTP error codes](http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html) along with some additional information.

* The error code is sent back as a status header.
* The body contains [application/api-problem+json](https://tools.ietf.org/html/draft-nottingham-http-problem-03) describing the problem in detail.

An error could for example look like:

```Status: 403 Forbidden```
```{
    code: 403,
    problemType: 'http://example.com/probs/out-of-credit',
    title: 'You do not have enough credit.',
    detail: 'Your current balance is 30, but that costs 50.',
    problemInstance: 'http://example.net/account/12345/msgs/abc',
    balance: 30,
    accounts: ['http://example.net/account/12345',
               'http://example.net/account/67890']
 }```
