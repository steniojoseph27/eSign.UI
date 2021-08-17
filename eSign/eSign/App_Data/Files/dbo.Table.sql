CREATE TABLE [dbo].[Recipient](  
[Id] [bigint] IDENTITY(1,1) NOT NULL,  
[Title] [nvarchar](250) NULL,  
[Email] [nvarchar](300) NULL,  
[EnvelopeID] [nvarchar](max) NULL,
[Signatories] [nvarchar] (max) NULL,
[Description] [nvarchar](max) NULL,  
[Documents] [varbinary](max) NULL,    
[Status] [varchar](100) NULL,  
[CreationDate] [datetime] NULL,  
[UpdateOn] [datetime] NULL,  
CONSTRAINT [PK_Recipient] PRIMARY KEY CLUSTERED  
(  
   [Id] ASC  
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]  
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]  
GO  