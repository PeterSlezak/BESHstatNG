# Planning and architecture roadmap

This directory contains the long-term design and migration proposal for a portable BESHStatNG statistical engine, reusable spreadsheet application layer, preserved Windows Excel-DNA product and future Office.js/WebAssembly product. The main document is [`BESHStatNG_CrossPlatform_MasterPlan.md`](BESHStatNG_CrossPlatform_MasterPlan.md).

The plan is a **development guide**, not a statement of which capabilities have already shipped. For the present implementation, consult the checked-in source, each project's README, owner acceptance records and the relevant engineering reports in [`../docs/architecture/`](../docs/architecture/README.md). Planning milestones and acceptance statuses may change as implementation and live host tests progress.

An accompanying Windows portability audit identifies WinForms, UDF and charting code that should be separated into Core/Common versus host-specific adapters. Work on Windows compatibility and new Windows features should preserve reusable statistical and output contracts for later Office.js adoption. LibreOffice compatibility is an architectural consideration only; no Python/UNO bridge implementation is currently part of the work.

When updating the roadmap, reconcile it with actual project targets, registered analysis handlers, supported Office UI, fixture coverage and the latest owner-reported Excel smoke tests. Keep proposed features visibly distinct from production functionality.
