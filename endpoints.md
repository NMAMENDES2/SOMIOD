ApplicationController:

GET - api/somiod/ header: application
GET(NAME) - api/somiod/{application}
POST - api/somiod/
PUT - api/somiod/{application}
DELETE - api/somiod/{application}

ContainerController:

GET - api/somiod header: container
GET(NAME) - api/somiod/{application}/{container}
POST - api/somiod/{application}
PUT - api/somiod/{application}/{container}
DELETE - api/somiod/{application}/{container}

RecordController:

GET - api/somiod header - record
GET(NAME) - api/somiod/{application}/{container}/record/{record}
POST - api/somiod/{application}/{container}/record
DELETE - api/somiod/{application}/{container}/record/{record}

NotificationController:

GET - api/somiod header - notification
GET(NAME) - api/somiod/{application}/{container}/notif/{notification}
POST - api/somiod/{application}/{container}/notif
DELETE - api/somiod/{applicaition}/{container}/notif/{notification}

Nós temos de ver o res_type que vem no body