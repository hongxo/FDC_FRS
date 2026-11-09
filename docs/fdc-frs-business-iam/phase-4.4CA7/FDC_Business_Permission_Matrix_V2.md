# FDC Business Permission Matrix V2

Dashboard → dashboard.view → SQL count đã scope; Stations → station.view; IED → ied.view; record metadata → records.events.view; downloadable-files và download → records.download độc lập. Tất cả là candidate mapping. Permission và GrantFunction phải cùng UserRole/StationGrant, SQL xác minh object station. Download audit/outbox ghi prepared. Collector và Lab CATALOG_GAP.
