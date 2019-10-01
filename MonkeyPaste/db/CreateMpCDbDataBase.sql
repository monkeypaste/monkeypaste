-- detach database MpDb;
-- attach database 'C:\Users\tkefauver\Dropbox\MonkeyPaste\MonkeyPaste\MonkeyPaste\db\mpdb.db' as MpDb;

-- DeviceType is iphone,ipad,android,tablet,pc,mac
create table MpDeviceType( 
pk_MpDeviceTypeId integer not null primary key autoincrement, 
DeviceTypeName varchar(15)
);
insert into MpDeviceType(DeviceTypeName)
values ('windows'),('mac'),('android'),('iphone'),('ipad'),('tablet');

-- patformtype: ios,android,windows,macMpClientId
create table MpPlatformType( 
pk_MpPlatformTypeId integer not null primary key autoincrement, 
PlatformName varchar(30) not null
);
insert into MpPlatformType(PlatformName)
values ('ios'),('android'),('windows'),('mac');

-- platform windows(type) version 7,9,10,etc.
create table MpPlatform( 
pk_MpPlatformId integer not null primary key autoincrement, 
fk_MpPlatformTypeId integer not null,
fk_MpDeviceTypeId integer not null,
Version varchar(30), 
constraint fk_MpPlatform_MpPlatformType foreign key(fk_MpPlatformTypeId) references MpPlatformType(pk_MpPlatformTypeId),
constraint fk_MpPlatform_MpDeviceType foreign key(fk_MpDeviceTypeId) references MpDeviceType(pk_MpDeviceTypeId)
);


create table MpClient(
pk_MpClientId integer not null primary key autoincrement,
fk_MpPlatformId integer not null,
Ip4Address varchar(30),
LoginDateTime datetime not null,
LogoutDateTime datetime,
constraint fk_MpClient_MpPlatform foreign key (fk_MpPlatformId) references MpPlatform(pk_MpPlatformId)
);
-- should be text, rich text, html, image, file list
create table MpCopyItemType(
pk_MpCopyItemTypeId integer not null primary key autoincrement,
TypeName varchar(20)
);
insert into MpCopyItemType(TypeName) 
values ('text'),('rich_text'),('html_text'),('image'),('file_list');

-- represents an application on users machine calling copy command (Urlpath may store webbrowser tab at some pointeger)
create table MpApp (
pk_MpAppId integer not null primary key autoincrement,
SourcePath varchar(255) not null,
SourceIcon longblob
);

create table MpCopyItem(
pk_MpCopyItemId integer not null primary key autoincrement,
fk_MpCopyItemTypeId integer not null,
fk_MpClientId integer not null,
fk_MpAppId integer not null,
CopyDateTime datetime not null default current_timestamp,
constraint fk_MpCopyItem_MpCopyItemType foreign key (fk_MpCopyItemTypeId) references MpCopyItemType(pk_MpCopyItemTypeId),
constraint fk_MpCopyItem_MpClient foreign key (fk_MpClientId) references MpClient(pk_MpClientId),
constraint fk_MpCopyItem_MpApp foreign key (fk_MpAppId) references MpApp(pk_MpAppId)
);

create table MpTextItem(
pk_MpTextItemId integer not null primary key autoincrement,
fk_MpCopyItemId integer not null,
ItemText longtext not null,
constraint fk_MpTextItem_MpCopyItem foreign key (fk_MpCopyItemId) references MpCopyItem(pk_MpCopyItemId)
);
create table MpRichTextItem(
pk_MpRichTextItemId integer not null primary key autoincrement,
fk_MpCopyItemId integer not null,
ItemRichText longtext not null,
ItemText longtext not null,
constraint fk_MpRichTextItem_MpCopyItem foreign key (fk_MpCopyItemId) references MpCopyItem(pk_MpCopyItemId)
);
create table MpHtmlTextItem(
pk_MpHtmlTextItemId integer not null primary key autoincrement,
fk_MpCopyItemId integer not null,
ItemHtmlText longtext not null,
ItemText longtext not null,
constraint fk_MpHtmlTextItem_MpCopyItem foreign key (fk_MpCopyItemId) references MpCopyItem(pk_MpCopyItemId)
);
create table MpImageCopyItem(
pk_MpImageCopyItemId integer not null primary key autoincrement,
fk_MpCopyItemId integer not null,
ImageBlobGenCSharpModel longblob not null,
constraint fk_MpImageCopyItem_MpCopyItem foreign key (fk_MpCopyItemId) references MpCopyItem(pk_MpCopyItemId)
);

create table MpFileDropListCopyItem(
pk_MpFileDropListCopyItemId integer not null primary key autoincrement,
fk_MpCopyItemId integer not null,
constraint fk_MpFileDropListCopyItem_MpCopyItem foreign key (fk_MpCopyItemId) references MpCopyItem(pk_MpCopyItemId)
);
create table MpFileDropItem(
pk_MpFileDropItemId integer not null primary key autoincrement,
fk_MpFileDropListCopyItemId integer not null,
DropPath varchar(255) not null,
constraint fk_MpFileDropItem_MpFileDropListCopyItem foreign key (fk_MpFileDropListCopyItemId) references MpFileDropListCopyItem(pk_MpFileDropListCopyItemId)
);

create table MpPasteHistory(
pk_MpPasteHistoryId integer not null primary key autoincrement,
fk_MpCopyItemId integer not null,
fk_MpClientId integer not null,
fk_MpAppId integer not null,
PasteDateTime datetime not null,
constraint fk_MpPasteHistory_MpCopyItem foreign key (fk_MpCopyItemId) references MpCopyItem(pk_MpCopyItemId),
constraint fk_MpPasteHistory_MpClient foreign key (fk_MpClientId) references MpClient(pk_MpClientId),
constraint fk_MpPasteHistory_MpApp foreign key (fk_MpAppId) references MpApp(pk_MpAppId)
);