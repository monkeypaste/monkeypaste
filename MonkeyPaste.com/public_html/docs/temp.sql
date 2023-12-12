CREATE TABLE [dbo].[Product](
    [Id] [int] IDENTITY(1, 1) NOT NULL,
    [Name] [nvarchar](100) NOT NULL,
    [Description] [nvarchar](100) NULL
) ON [PRIMARY]