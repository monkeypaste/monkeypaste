-- Script Date: 9/28/2019 8:44 PM  - ErikEJ.SqlCeScripting version 3.5.2.81
-- Database information:
-- Database: C:\Users\tkefauver\Dropbox\Dev\MonkeyPaste\MonkeyPaste\bin\Debug\mp.db
-- ServerVersion: 3.27.2
-- DatabaseSize: 128 KB
-- Created: 9/21/2019 8:09 AM

-- User Table information:
-- Number of tables: 20
-- MpApp: -1 row(s)
-- MpClient: -1 row(s)
-- MpCommand: -1 row(s)
-- MpCommandType: -1 row(s)
-- MpCopyItem: -1 row(s)
-- MpCopyItemType: -1 row(s)
-- MpDeviceType: -1 row(s)
-- MpFileDropListItem: -1 row(s)
-- MpFileDropListSubItem: -1 row(s)
-- MpHotKey: -1 row(s)
-- MpIcon: -1 row(s)
-- MpImageItem: -1 row(s)
-- MpPasteHistory: -1 row(s)
-- MpPlatform: -1 row(s)
-- MpPlatformType: -1 row(s)
-- MpSetting: -1 row(s)
-- MpSubTextToken: -1 row(s)
-- MpTag: -1 row(s)
-- MpTagType: -1 row(s)
-- MpTextItem: -1 row(s)

-- Warning - constraint: MpCommand Parent Columns and Child Columns don't have type-matching columns.
-- Warning - constraint: MpCommand Parent Columns and Child Columns don't have type-matching columns.
SELECT 1;
PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE [MpTagType] (
  [pk_MpTagTypeId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [TagTypeName] text NOT NULL
);
CREATE TABLE [MpTag] (
  [pk_MpTagId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [fk_MpTagTypeId] bigint NOT NULL
, [TagName] text NOT NULL
, [TagHexColor] text NOT NULL
, CONSTRAINT [FK_MpTag_0_0] FOREIGN KEY ([fk_MpTagTypeId]) REFERENCES [MpTagType] ([pk_MpTagTypeId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
CREATE TABLE [MpSetting] (
  [pk_MpSettingId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [SettingName] text NOT NULL
, [SettingValueType] text NOT NULL
, [SettingValue] text NULL
, [SettingDefaultValue] text NULL
);
CREATE TABLE [MpPlatformType] (
  [pk_MpPlatformTypeId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [PlatformName] nvarchar(30) NOT NULL COLLATE NOCASE
);
CREATE TABLE [MpIcon] (
  [pk_MpIconId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [IconBlob] image NOT NULL
);
CREATE TABLE [MpHotKey] (
  [pk_MpHotKeyId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [KeyList] text NULL
, [ModList] text NULL
);
CREATE TABLE [MpDeviceType] (
  [pk_MpDeviceTypeId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [DeviceTypeName] nvarchar(15) NULL COLLATE NOCASE
);
CREATE TABLE [MpPlatform] (
  [pk_MpPlatformId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [fk_MpPlatformTypeId] bigint NOT NULL
, [fk_MpDeviceTypeId] bigint NOT NULL
, [Version] nvarchar(30) NULL COLLATE NOCASE
, CONSTRAINT [FK_MpPlatform_0_0] FOREIGN KEY ([fk_MpDeviceTypeId]) REFERENCES [MpDeviceType] ([pk_MpDeviceTypeId]) ON DELETE NO ACTION ON UPDATE NO ACTION
, CONSTRAINT [FK_MpPlatform_1_0] FOREIGN KEY ([fk_MpPlatformTypeId]) REFERENCES [MpPlatformType] ([pk_MpPlatformTypeId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
CREATE TABLE [MpCopyItemType] (
  [pk_MpCopyItemTypeId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [TypeName] nvarchar(20) NULL COLLATE NOCASE
);
CREATE TABLE [MpCommandType] (
  [pk_MpCommandTypeId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [CommandName] text NOT NULL
);
CREATE TABLE [MpCommand] (
  [pk_MpCommandId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [fk_MpCommandTypeId] int NOT NULL
, [fk_MpHotKey] int NOT NULL
, CONSTRAINT [FK_MpCommand_0_0] FOREIGN KEY ([fk_MpHotKey]) REFERENCES [MpHotKey] ([pk_MpHotKeyId]) ON DELETE NO ACTION ON UPDATE NO ACTION
, CONSTRAINT [FK_MpCommand_1_0] FOREIGN KEY ([fk_MpCommandTypeId]) REFERENCES [MpCommandType] ([pk_MpCommandTypeId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
CREATE TABLE [MpClient] (
  [pk_MpClientId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [fk_MpPlatformId] bigint NOT NULL
, [Ip4Address] nvarchar(30) NULL COLLATE NOCASE
, [AccessToken] nvarchar(255) NULL COLLATE NOCASE
, [LoginDateTime] datetime NOT NULL
, [LogoutDateTime] datetime NULL
, CONSTRAINT [FK_MpClient_0_0] FOREIGN KEY ([fk_MpPlatformId]) REFERENCES [MpPlatform] ([pk_MpPlatformId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
CREATE TABLE [MpApp] (
  [pk_MpAppId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [fk_MpIconId] bigint NOT NULL
, [SourcePath] nvarchar(255) NOT NULL COLLATE NOCASE
, [IsAppRejected] bigint NOT NULL
, [fk_ColorId] bigint DEFAULT (1) NOT NULL
, CONSTRAINT [FK_MpApp_0_0] FOREIGN KEY ([fk_MpIconId]) REFERENCES [MpIcon] ([pk_MpIconId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
CREATE TABLE [MpCopyItem] (
  [pk_MpCopyItemId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [fk_MpCopyItemTypeId] bigint NOT NULL
, [fk_MpClientId] bigint NOT NULL
, [fk_MpAppId] bigint NOT NULL
, [Title] nvarchar(2147483647) NULL COLLATE NOCASE
, [CopyDateTime] datetime DEFAULT (current_timestamp) NOT NULL
, [Color] text NULL
, CONSTRAINT [FK_MpCopyItem_0_0] FOREIGN KEY ([fk_MpAppId]) REFERENCES [MpApp] ([pk_MpAppId]) ON DELETE NO ACTION ON UPDATE NO ACTION
, CONSTRAINT [FK_MpCopyItem_1_0] FOREIGN KEY ([fk_MpClientId]) REFERENCES [MpClient] ([pk_MpClientId]) ON DELETE NO ACTION ON UPDATE NO ACTION
, CONSTRAINT [FK_MpCopyItem_2_0] FOREIGN KEY ([fk_MpCopyItemTypeId]) REFERENCES [MpCopyItemType] ([pk_MpCopyItemTypeId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
CREATE TABLE [MpSubTextToken] (
  [pk_MpSubTextTokenId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [fk_MpCopyItemId] bigint NOT NULL
, [fk_MpCopyItemTypeId] bigint NOT NULL
, [StartIdx] bigint NOT NULL
, [EndIdx] bigint NOT NULL
, [InstanceIdx] int NOT NULL
, CONSTRAINT [FK_MpSubTextToken_0_0] FOREIGN KEY ([fk_MpCopyItemTypeId]) REFERENCES [MpCopyItemType] ([pk_MpCopyItemTypeId]) ON DELETE NO ACTION ON UPDATE NO ACTION
, CONSTRAINT [FK_MpSubTextToken_1_0] FOREIGN KEY ([fk_MpCopyItemId]) REFERENCES [MpCopyItem] ([pk_MpCopyItemId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
CREATE TABLE [MpPasteHistory] (
  [pk_MpPasteHistoryId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [fk_MpCopyItemId] bigint NOT NULL
, [fk_MpClientId] bigint NOT NULL
, [fk_MpAppId] bigint NOT NULL
, [PasteDateTime] datetime NOT NULL
, CONSTRAINT [FK_MpPasteHistory_0_0] FOREIGN KEY ([fk_MpAppId]) REFERENCES [MpApp] ([pk_MpAppId]) ON DELETE NO ACTION ON UPDATE NO ACTION
, CONSTRAINT [FK_MpPasteHistory_1_0] FOREIGN KEY ([fk_MpClientId]) REFERENCES [MpClient] ([pk_MpClientId]) ON DELETE NO ACTION ON UPDATE NO ACTION
, CONSTRAINT [FK_MpPasteHistory_2_0] FOREIGN KEY ([fk_MpCopyItemId]) REFERENCES [MpCopyItem] ([pk_MpCopyItemId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
CREATE TABLE [MpFileDropListItem] (
  [pk_MpFileDropListItemId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [fk_MpCopyItemId] bigint NOT NULL
, CONSTRAINT [FK_MpFileDropListItem_0_0] FOREIGN KEY ([fk_MpCopyItemId]) REFERENCES [MpCopyItem] ([pk_MpCopyItemId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
CREATE TABLE [MpFileDropListSubItem] (
  [pk_MpFileDropListSubItemId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [fk_MpFileDropListItemId] bigint NOT NULL
, [ItemPath] nvarchar(255) NOT NULL COLLATE NOCASE
, CONSTRAINT [FK_MpFileDropListSubItem_0_0] FOREIGN KEY ([fk_MpFileDropListItemId]) REFERENCES [MpFileDropListItem] ([pk_MpFileDropListItemId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
CREATE TABLE [MpImageItem] (
  [pk_MpImageItemId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [fk_MpCopyItemId] bigint NOT NULL
, [ItemImage] longblob NOT NULL
, CONSTRAINT [FK_MpImageItem_0_0] FOREIGN KEY ([fk_MpCopyItemId]) REFERENCES [MpCopyItem] ([pk_MpCopyItemId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
CREATE TABLE [MpTextItem] (
  [pk_MpTextItemId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [fk_MpCopyItemId] bigint NOT NULL
, [ItemText] nvarchar(2147483647) NOT NULL COLLATE NOCASE
, CONSTRAINT [FK_MpTextItem_0_0] FOREIGN KEY ([fk_MpCopyItemId]) REFERENCES [MpCopyItem] ([pk_MpCopyItemId]) ON DELETE NO ACTION ON UPDATE NO ACTION
);
CREATE TRIGGER [fki_MpTag_fk_MpTagTypeId_MpTagType_pk_MpTagTypeId] BEFORE Insert ON [MpTag] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpTag violates foreign key constraint FK_MpTag_0_0') WHERE (SELECT pk_MpTagTypeId FROM MpTagType WHERE  pk_MpTagTypeId = NEW.fk_MpTagTypeId) IS NULL; END;
CREATE TRIGGER [fku_MpTag_fk_MpTagTypeId_MpTagType_pk_MpTagTypeId] BEFORE Update ON [MpTag] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpTag violates foreign key constraint FK_MpTag_0_0') WHERE (SELECT pk_MpTagTypeId FROM MpTagType WHERE  pk_MpTagTypeId = NEW.fk_MpTagTypeId) IS NULL; END;
CREATE TRIGGER [fki_MpPlatform_fk_MpDeviceTypeId_MpDeviceType_pk_MpDeviceTypeId] BEFORE Insert ON [MpPlatform] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpPlatform violates foreign key constraint FK_MpPlatform_0_0') WHERE (SELECT pk_MpDeviceTypeId FROM MpDeviceType WHERE  pk_MpDeviceTypeId = NEW.fk_MpDeviceTypeId) IS NULL; END;
CREATE TRIGGER [fku_MpPlatform_fk_MpDeviceTypeId_MpDeviceType_pk_MpDeviceTypeId] BEFORE Update ON [MpPlatform] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpPlatform violates foreign key constraint FK_MpPlatform_0_0') WHERE (SELECT pk_MpDeviceTypeId FROM MpDeviceType WHERE  pk_MpDeviceTypeId = NEW.fk_MpDeviceTypeId) IS NULL; END;
CREATE TRIGGER [fki_MpPlatform_fk_MpPlatformTypeId_MpPlatformType_pk_MpPlatformTypeId] BEFORE Insert ON [MpPlatform] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpPlatform violates foreign key constraint FK_MpPlatform_1_0') WHERE (SELECT pk_MpPlatformTypeId FROM MpPlatformType WHERE  pk_MpPlatformTypeId = NEW.fk_MpPlatformTypeId) IS NULL; END;
CREATE TRIGGER [fku_MpPlatform_fk_MpPlatformTypeId_MpPlatformType_pk_MpPlatformTypeId] BEFORE Update ON [MpPlatform] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpPlatform violates foreign key constraint FK_MpPlatform_1_0') WHERE (SELECT pk_MpPlatformTypeId FROM MpPlatformType WHERE  pk_MpPlatformTypeId = NEW.fk_MpPlatformTypeId) IS NULL; END;
CREATE TRIGGER [fki_MpCommand_fk_MpHotKey_MpHotKey_pk_MpHotKeyId] BEFORE Insert ON [MpCommand] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpCommand violates foreign key constraint FK_MpCommand_0_0') WHERE (SELECT pk_MpHotKeyId FROM MpHotKey WHERE  pk_MpHotKeyId = NEW.fk_MpHotKey) IS NULL; END;
CREATE TRIGGER [fku_MpCommand_fk_MpHotKey_MpHotKey_pk_MpHotKeyId] BEFORE Update ON [MpCommand] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpCommand violates foreign key constraint FK_MpCommand_0_0') WHERE (SELECT pk_MpHotKeyId FROM MpHotKey WHERE  pk_MpHotKeyId = NEW.fk_MpHotKey) IS NULL; END;
CREATE TRIGGER [fki_MpCommand_fk_MpCommandTypeId_MpCommandType_pk_MpCommandTypeId] BEFORE Insert ON [MpCommand] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpCommand violates foreign key constraint FK_MpCommand_1_0') WHERE (SELECT pk_MpCommandTypeId FROM MpCommandType WHERE  pk_MpCommandTypeId = NEW.fk_MpCommandTypeId) IS NULL; END;
CREATE TRIGGER [fku_MpCommand_fk_MpCommandTypeId_MpCommandType_pk_MpCommandTypeId] BEFORE Update ON [MpCommand] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpCommand violates foreign key constraint FK_MpCommand_1_0') WHERE (SELECT pk_MpCommandTypeId FROM MpCommandType WHERE  pk_MpCommandTypeId = NEW.fk_MpCommandTypeId) IS NULL; END;
CREATE TRIGGER [fki_MpClient_fk_MpPlatformId_MpPlatform_pk_MpPlatformId] BEFORE Insert ON [MpClient] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpClient violates foreign key constraint FK_MpClient_0_0') WHERE (SELECT pk_MpPlatformId FROM MpPlatform WHERE  pk_MpPlatformId = NEW.fk_MpPlatformId) IS NULL; END;
CREATE TRIGGER [fku_MpClient_fk_MpPlatformId_MpPlatform_pk_MpPlatformId] BEFORE Update ON [MpClient] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpClient violates foreign key constraint FK_MpClient_0_0') WHERE (SELECT pk_MpPlatformId FROM MpPlatform WHERE  pk_MpPlatformId = NEW.fk_MpPlatformId) IS NULL; END;
CREATE TRIGGER [fki_MpApp_fk_MpIconId_MpIcon_pk_MpIconId] BEFORE Insert ON [MpApp] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpApp violates foreign key constraint FK_MpApp_0_0') WHERE (SELECT pk_MpIconId FROM MpIcon WHERE  pk_MpIconId = NEW.fk_MpIconId) IS NULL; END;
CREATE TRIGGER [fku_MpApp_fk_MpIconId_MpIcon_pk_MpIconId] BEFORE Update ON [MpApp] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpApp violates foreign key constraint FK_MpApp_0_0') WHERE (SELECT pk_MpIconId FROM MpIcon WHERE  pk_MpIconId = NEW.fk_MpIconId) IS NULL; END;
CREATE TRIGGER [fki_MpCopyItem_fk_MpAppId_MpApp_pk_MpAppId] BEFORE Insert ON [MpCopyItem] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpCopyItem violates foreign key constraint FK_MpCopyItem_0_0') WHERE (SELECT pk_MpAppId FROM MpApp WHERE  pk_MpAppId = NEW.fk_MpAppId) IS NULL; END;
CREATE TRIGGER [fku_MpCopyItem_fk_MpAppId_MpApp_pk_MpAppId] BEFORE Update ON [MpCopyItem] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpCopyItem violates foreign key constraint FK_MpCopyItem_0_0') WHERE (SELECT pk_MpAppId FROM MpApp WHERE  pk_MpAppId = NEW.fk_MpAppId) IS NULL; END;
CREATE TRIGGER [fki_MpCopyItem_fk_MpClientId_MpClient_pk_MpClientId] BEFORE Insert ON [MpCopyItem] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpCopyItem violates foreign key constraint FK_MpCopyItem_1_0') WHERE (SELECT pk_MpClientId FROM MpClient WHERE  pk_MpClientId = NEW.fk_MpClientId) IS NULL; END;
CREATE TRIGGER [fku_MpCopyItem_fk_MpClientId_MpClient_pk_MpClientId] BEFORE Update ON [MpCopyItem] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpCopyItem violates foreign key constraint FK_MpCopyItem_1_0') WHERE (SELECT pk_MpClientId FROM MpClient WHERE  pk_MpClientId = NEW.fk_MpClientId) IS NULL; END;
CREATE TRIGGER [fki_MpCopyItem_fk_MpCopyItemTypeId_MpCopyItemType_pk_MpCopyItemTypeId] BEFORE Insert ON [MpCopyItem] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpCopyItem violates foreign key constraint FK_MpCopyItem_2_0') WHERE (SELECT pk_MpCopyItemTypeId FROM MpCopyItemType WHERE  pk_MpCopyItemTypeId = NEW.fk_MpCopyItemTypeId) IS NULL; END;
CREATE TRIGGER [fku_MpCopyItem_fk_MpCopyItemTypeId_MpCopyItemType_pk_MpCopyItemTypeId] BEFORE Update ON [MpCopyItem] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpCopyItem violates foreign key constraint FK_MpCopyItem_2_0') WHERE (SELECT pk_MpCopyItemTypeId FROM MpCopyItemType WHERE  pk_MpCopyItemTypeId = NEW.fk_MpCopyItemTypeId) IS NULL; END;
CREATE TRIGGER [fki_MpSubTextToken_fk_MpCopyItemTypeId_MpCopyItemType_pk_MpCopyItemTypeId] BEFORE Insert ON [MpSubTextToken] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpSubTextToken violates foreign key constraint FK_MpSubTextToken_0_0') WHERE (SELECT pk_MpCopyItemTypeId FROM MpCopyItemType WHERE  pk_MpCopyItemTypeId = NEW.fk_MpCopyItemTypeId) IS NULL; END;
CREATE TRIGGER [fku_MpSubTextToken_fk_MpCopyItemTypeId_MpCopyItemType_pk_MpCopyItemTypeId] BEFORE Update ON [MpSubTextToken] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpSubTextToken violates foreign key constraint FK_MpSubTextToken_0_0') WHERE (SELECT pk_MpCopyItemTypeId FROM MpCopyItemType WHERE  pk_MpCopyItemTypeId = NEW.fk_MpCopyItemTypeId) IS NULL; END;
CREATE TRIGGER [fki_MpSubTextToken_fk_MpCopyItemId_MpCopyItem_pk_MpCopyItemId] BEFORE Insert ON [MpSubTextToken] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpSubTextToken violates foreign key constraint FK_MpSubTextToken_1_0') WHERE (SELECT pk_MpCopyItemId FROM MpCopyItem WHERE  pk_MpCopyItemId = NEW.fk_MpCopyItemId) IS NULL; END;
CREATE TRIGGER [fku_MpSubTextToken_fk_MpCopyItemId_MpCopyItem_pk_MpCopyItemId] BEFORE Update ON [MpSubTextToken] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpSubTextToken violates foreign key constraint FK_MpSubTextToken_1_0') WHERE (SELECT pk_MpCopyItemId FROM MpCopyItem WHERE  pk_MpCopyItemId = NEW.fk_MpCopyItemId) IS NULL; END;
CREATE TRIGGER [fki_MpPasteHistory_fk_MpAppId_MpApp_pk_MpAppId] BEFORE Insert ON [MpPasteHistory] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpPasteHistory violates foreign key constraint FK_MpPasteHistory_0_0') WHERE (SELECT pk_MpAppId FROM MpApp WHERE  pk_MpAppId = NEW.fk_MpAppId) IS NULL; END;
CREATE TRIGGER [fku_MpPasteHistory_fk_MpAppId_MpApp_pk_MpAppId] BEFORE Update ON [MpPasteHistory] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpPasteHistory violates foreign key constraint FK_MpPasteHistory_0_0') WHERE (SELECT pk_MpAppId FROM MpApp WHERE  pk_MpAppId = NEW.fk_MpAppId) IS NULL; END;
CREATE TRIGGER [fki_MpPasteHistory_fk_MpClientId_MpClient_pk_MpClientId] BEFORE Insert ON [MpPasteHistory] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpPasteHistory violates foreign key constraint FK_MpPasteHistory_1_0') WHERE (SELECT pk_MpClientId FROM MpClient WHERE  pk_MpClientId = NEW.fk_MpClientId) IS NULL; END;
CREATE TRIGGER [fku_MpPasteHistory_fk_MpClientId_MpClient_pk_MpClientId] BEFORE Update ON [MpPasteHistory] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpPasteHistory violates foreign key constraint FK_MpPasteHistory_1_0') WHERE (SELECT pk_MpClientId FROM MpClient WHERE  pk_MpClientId = NEW.fk_MpClientId) IS NULL; END;
CREATE TRIGGER [fki_MpPasteHistory_fk_MpCopyItemId_MpCopyItem_pk_MpCopyItemId] BEFORE Insert ON [MpPasteHistory] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpPasteHistory violates foreign key constraint FK_MpPasteHistory_2_0') WHERE (SELECT pk_MpCopyItemId FROM MpCopyItem WHERE  pk_MpCopyItemId = NEW.fk_MpCopyItemId) IS NULL; END;
CREATE TRIGGER [fku_MpPasteHistory_fk_MpCopyItemId_MpCopyItem_pk_MpCopyItemId] BEFORE Update ON [MpPasteHistory] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpPasteHistory violates foreign key constraint FK_MpPasteHistory_2_0') WHERE (SELECT pk_MpCopyItemId FROM MpCopyItem WHERE  pk_MpCopyItemId = NEW.fk_MpCopyItemId) IS NULL; END;
CREATE TRIGGER [fki_MpFileDropListItem_fk_MpCopyItemId_MpCopyItem_pk_MpCopyItemId] BEFORE Insert ON [MpFileDropListItem] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpFileDropListItem violates foreign key constraint FK_MpFileDropListItem_0_0') WHERE (SELECT pk_MpCopyItemId FROM MpCopyItem WHERE  pk_MpCopyItemId = NEW.fk_MpCopyItemId) IS NULL; END;
CREATE TRIGGER [fku_MpFileDropListItem_fk_MpCopyItemId_MpCopyItem_pk_MpCopyItemId] BEFORE Update ON [MpFileDropListItem] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpFileDropListItem violates foreign key constraint FK_MpFileDropListItem_0_0') WHERE (SELECT pk_MpCopyItemId FROM MpCopyItem WHERE  pk_MpCopyItemId = NEW.fk_MpCopyItemId) IS NULL; END;
CREATE TRIGGER [fki_MpFileDropListSubItem_fk_MpFileDropListItemId_MpFileDropListItem_pk_MpFileDropListItemId] BEFORE Insert ON [MpFileDropListSubItem] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpFileDropListSubItem violates foreign key constraint FK_MpFileDropListSubItem_0_0') WHERE (SELECT pk_MpFileDropListItemId FROM MpFileDropListItem WHERE  pk_MpFileDropListItemId = NEW.fk_MpFileDropListItemId) IS NULL; END;
CREATE TRIGGER [fku_MpFileDropListSubItem_fk_MpFileDropListItemId_MpFileDropListItem_pk_MpFileDropListItemId] BEFORE Update ON [MpFileDropListSubItem] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpFileDropListSubItem violates foreign key constraint FK_MpFileDropListSubItem_0_0') WHERE (SELECT pk_MpFileDropListItemId FROM MpFileDropListItem WHERE  pk_MpFileDropListItemId = NEW.fk_MpFileDropListItemId) IS NULL; END;
CREATE TRIGGER [fki_MpImageItem_fk_MpCopyItemId_MpCopyItem_pk_MpCopyItemId] BEFORE Insert ON [MpImageItem] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpImageItem violates foreign key constraint FK_MpImageItem_0_0') WHERE (SELECT pk_MpCopyItemId FROM MpCopyItem WHERE  pk_MpCopyItemId = NEW.fk_MpCopyItemId) IS NULL; END;
CREATE TRIGGER [fku_MpImageItem_fk_MpCopyItemId_MpCopyItem_pk_MpCopyItemId] BEFORE Update ON [MpImageItem] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpImageItem violates foreign key constraint FK_MpImageItem_0_0') WHERE (SELECT pk_MpCopyItemId FROM MpCopyItem WHERE  pk_MpCopyItemId = NEW.fk_MpCopyItemId) IS NULL; END;
CREATE TRIGGER [fki_MpTextItem_fk_MpCopyItemId_MpCopyItem_pk_MpCopyItemId] BEFORE Insert ON [MpTextItem] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table MpTextItem violates foreign key constraint FK_MpTextItem_0_0') WHERE (SELECT pk_MpCopyItemId FROM MpCopyItem WHERE  pk_MpCopyItemId = NEW.fk_MpCopyItemId) IS NULL; END;
CREATE TRIGGER [fku_MpTextItem_fk_MpCopyItemId_MpCopyItem_pk_MpCopyItemId] BEFORE Update ON [MpTextItem] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table MpTextItem violates foreign key constraint FK_MpTextItem_0_0') WHERE (SELECT pk_MpCopyItemId FROM MpCopyItem WHERE  pk_MpCopyItemId = NEW.fk_MpCopyItemId) IS NULL; END;
COMMIT;

