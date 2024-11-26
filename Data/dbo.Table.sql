CREATE TABLE [dbo].[Container]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [name] VARCHAR(50) NOT NULL UNIQUE, 
    [creation_datetime] DATETIME2 NOT NULL DEFAULT GETDATE(), 
    [parent] INT NOT NULL
)


CREATE TABLE [dbo].[Notification] (
    [Id]                INT IDENTITY(1,1)          NOT NULL,
    [name]              VARCHAR (50)  NOT NULL,
    [creation_datetime] DATETIME2 (7) DEFAULT (getdate()) NOT NULL,
    [parent]            INT           NOT NULL,
    [event]             INT           NOT NULL,
    [endpoint]          VARCHAR (255) NOT NULL,
    [enabled]           BIT           DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    UNIQUE NONCLUSTERED ([name] ASC),
    CONSTRAINT [FK_Notification_ToContainer] FOREIGN KEY ([parent]) REFERENCES [dbo].[Container] ([Id])
);





CREATE TABLE [dbo].[Record] (
    [Id]                INT IDENTITY(1,1)          NOT NULL,
    [name]              VARCHAR (50)  NOT NULL,
    [content]           VARCHAR (50)  NOT NULL,
    [creation_datetime] DATETIME2 (7) DEFAULT (getdate()) NOT NULL,
    [parent]            INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    UNIQUE NONCLUSTERED ([name] ASC),
    CONSTRAINT [FK_Record_ToContainer] FOREIGN KEY ([parent]) REFERENCES [dbo].[Container] ([Id])
);


CREATE TABLE [dbo].[Application] (
    [Id]                INT IDENTITY(1,1)           NOT NULL,
    [name]              VARCHAR (50)  NOT NULL,
    [creation_datetime] DATETIME2 (7) DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    UNIQUE NONCLUSTERED ([name] ASC)
);





