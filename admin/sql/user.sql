

INSERT INTO public."Users"
("UserName", "PasswordHash", "Salt", "Role", "CreatedAt")
values
('user', 'knEX8Xp8Waa9/P89Qxy7DbEd5oumdgBA0/EVIh3XoFs=', 'FQjkR08swP471JN0g4F3UwG9zk61jjbTXx37rEHn17s=', 'User', CURRENT_TIMESTAMP),
('admin', '/WoSMsEmxNWymBFOfmcgNHAG/k0r2wGlNhDdEiHhKB8=', '5Wqvx4NTar97BqqKy7ozPSpV8Giys2GIt+fmIKYbAxI=', 'Admin', CURRENT_TIMESTAMP);
