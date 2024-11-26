CREATE TABLE [dbo].[Container]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [name] VARCHAR(50) NOT NULL, 
    [creation_datetime] DATETIME2 NOT NULL DEFAULT GETDATE(), 
    [parent] INT NOT NULL
)


CREATE TABLE [dbo].[Notification]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [name] VARCHAR(50) NOT NULL,
	[creation_datetime] DATETIME2 NOT NULL DEFAULT GETDATE(), 
    [parent] INT NOT NULL, 
    [event] INT NOT NULL, 
    [endpoint] VARCHAR(50) NOT NULL, 
    [enabled] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_Notification_ToContainer] FOREIGN KEY ([parent]) REFERENCES [Container]([id]),

)



CREATE TABLE [dbo].[Record]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [name] VARCHAR(50) NOT NULL, 
    [content] VARCHAR(50) NOT NULL,
	[creation_datetime] DATETIME2 NOT NULL DEFAULT GETDATE(), 
    [parent] INT NOT NULL, 
    CONSTRAINT [FK_Record_ToContainer] FOREIGN KEY ([parent]) REFERENCES [Container]([id]),
)



CREATE TABLE [dbo].[Application] (
    [Id]                INT           NOT NULL,
    [name]              VARCHAR (50)  NOT NULL,
    [creation_datetime] DATETIME2 (7) DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


