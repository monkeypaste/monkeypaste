--create database MpSDB;
use mps;
-- UserStates: Pending,Active,Reset,Inactive,Deactivated
create table MpUserState(
MpUserStateId int not null auto_increment primary key,
UserState varchar(255)
);
insert into MpUserState(UserState) 
values ('prepending'),('pending'),('active'),('reset'),('inactive'),('deactivated');

create table MpUser(
MpUserId int not null auto_increment primary key,
MpUserStateId int not null,
MpClientId int not null,
Email varchar(255) unique not null,
Pword varchar(255) not null,
foreign key (MpUserStateId) references MpUserState(MpUserStateId)
);
-- groups: Anonymous,Trial,PostTrial,Basic (10mb), Silver(100mb), Gold (1 gig), Admin
create table MpGroup (
MpGroupId int not null auto_increment primary key,
GroupName varchar(30) not null
);
insert into MpGroup(GroupName)
values ('guest'),('trial'),('post_trial'),('basic'),('silver'),('gold'),('admin');

-- associates the user to a group type
create table MpUserGroup (
MpUserGroupId int not null auto_increment primary key,
MpUserId int not null,
MpGroupId int not null,
foreign key (MpUserId) references MpUser(MpUserId),
foreign key (MpGroupId) references MpGroup(MpGroupId)
);
-- ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

-- DeviceType is iphone,ipad,android,tablet,pc,mac
create table MpClientDevice( 
MpClientDeviceId int not null auto_increment primary key, 
DeviceTypeName varchar(100)
);
insert into MpClientDevice(DeviceTypeName)
values ('windows'),('mac'),('androidMobileTypeN '),('iphoneTypeN'),('ipadTypeN'),('androidTableTypeN'),('linux');

-- patformtype: ios,android,windows,mac
create table MpClientOSType( 
MpClientOSTypeId int not null auto_increment primary key, 
OSName varchar(30) not null
);
insert into MpClientOSType(OSName)
values ('ios'),('android'),('windows'),('mac');

-- Windows|w_build2.18|10.1646|64|
create table MpClientOS( 
MpClientOSId int auto_increment not null primary key, 
MpClientOSTypeId int not null,
MpClientDeviceId int not null,
OSVersion varchar(30),
Bittism int,
foreign key(MpClientOSTypeId) references MpClientOSType(MpClientOSTypeId),
foreign key(MpClientDeviceId) references MpClientDevice(MpClientDeviceId)
);
-- 
-- |iPad4|build1.68|2019-08-05|
create table MpClientVersion(
MpClientVersionId int not null auto_increment primary key,
MpClientOSId int not null,
ClientVersion varchar(30) not null,
CreatedDateTime datetime not null,
foreign key(MpClientOSId) references MpClientOS(MpClientOSId)
);
-- the app on the users device
create table MpUserClient(
MpUserClientId int not null auto_increment primary key,
MpUserId int not null,
MpDeviceGUID varchar(500) not null,
IsAuthenticated bit not null default 0,
AuthenticationCode varchar(10),
RequestedAuthenticationDateTime datetime not null,
ReceivedAuthenticationDateTime datetime,
foreign key (MpUserId) references MpUser(MpUserId)
);
-- stores active user and their apps
create table MpUserSession(
MpUserSessionId int not null auto_increment primary key,
MpUserClientId int not null,
MpClientVersionId int not null,
Ip4Address varchar(30) not null,
LoginDateTime datetime not null,
foreign key (MpUserClientId) references MpUserClient(MpUserClientId),
foreign key (MpClientVersionId) references MpClientVersion(MpClientVersionId)
);