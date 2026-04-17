INSERT INTO public."MonitoringPost"
(
    "Name", "Description", "Longitude", "Latitude", "IsMobile", "IsActive", "CreatedAt", "UpdatedAt", "Address", "PollingIntervalSeconds", "LastPolledAt"
)
VALUES
(
    'АДМС', 'Автоматическая дорожная метео станция', 37.485538, 55.862116, false, true, now(), now(), 'г. Москва. ул. Лавочкина 19', 60, null
);