# Browser BFCache and session

`business.mjs` and `lab.mjs` already clear/abort on pagehide and revalidate on pageshow. The browser regression tests multiple session/security paths. A10-055 stays open until a genuine persisted pageshow and delayed logout response are observed together with an in-flight protected detail response. Record persisted flag, browser trace and DOM state before and after delayed responses, and assert cookie replay denied server-side. Browser history behavior needs confirmation on the target browser build.
