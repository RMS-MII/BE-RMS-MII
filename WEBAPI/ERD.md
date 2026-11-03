##Ini adalah struktur ERD yang akan dibuat dan menjadi acuan development

{
"version": "32.6.9",
"pages": [
{
"id": "SHmtY8e3uzaJ9BATUX7A",
"name": "ERD",
"cells": [
{
"id": "1",
"type": "layer"
},
{
"id": "nfNeyiAOdsavxUQbhxos-6",
"type": "edge",
"parent": "1",
"source": "Jt6paovLrLvcBHmawJKR-97",
"target": "nfNeyiAOdsavxUQbhxos-1"
},
{
"id": "9bh975Iajj_f-3G999F8-1",
"type": "node",
"parent": "1",
"label": "Employee"
},
{
"id": "9bh975Iajj_f-3G999F8-2",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-1"
},
{
"id": "9bh975Iajj_f-3G999F8-3",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-2",
"label": "PK"
},
{
"id": "9bh975Iajj_f-3G999F8-4",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-2",
"label": "Id GUID"
},
{
"id": "aBztl0BIyrloTKxhFtse-3",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-1"
},
{
"id": "aBztl0BIyrloTKxhFtse-4",
"type": "node",
"parent": "aBztl0BIyrloTKxhFtse-3",
"label": "FK"
},
{
"id": "aBztl0BIyrloTKxhFtse-5",
"type": "node",
"parent": "aBztl0BIyrloTKxhFtse-3",
"label": "ManagerId GUID"
},
{
"id": "aBztl0BIyrloTKxhFtse-10",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-1"
},
{
"id": "aBztl0BIyrloTKxhFtse-11",
"type": "node",
"parent": "aBztl0BIyrloTKxhFtse-10",
"label": "FK"
},
{
"id": "aBztl0BIyrloTKxhFtse-12",
"type": "node",
"parent": "aBztl0BIyrloTKxhFtse-10",
"label": "DepartmentId GUID"
},
{
"id": "9bh975Iajj_f-3G999F8-5",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-1"
},
{
"id": "9bh975Iajj_f-3G999F8-6",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-5",
"label": "*"
},
{
"id": "9bh975Iajj_f-3G999F8-7",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-5",
"label": "EmployeeNumber : nvarchar(50)"
},
{
"id": "9bh975Iajj_f-3G999F8-8",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-1"
},
{
"id": "9bh975Iajj_f-3G999F8-9",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-8",
"label": "*"
},
{
"id": "9bh975Iajj_f-3G999F8-10",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-8",
"label": "FullName : nvarchar(50)"
},
{
"id": "9bh975Iajj_f-3G999F8-11",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-1"
},
{
"id": "9bh975Iajj_f-3G999F8-12",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-11",
"label": "* U"
},
{
"id": "9bh975Iajj_f-3G999F8-13",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-11",
"label": "Email : nvarchar(50)"
},
{
"id": "aBztl0BIyrloTKxhFtse-7",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-1"
},
{
"id": "aBztl0BIyrloTKxhFtse-8",
"type": "node",
"parent": "aBztl0BIyrloTKxhFtse-7",
"label": "*"
},
{
"id": "aBztl0BIyrloTKxhFtse-9",
"type": "node",
"parent": "aBztl0BIyrloTKxhFtse-7",
"label": "Gender : enum"
},
{
"id": "z8z-17g71uec0qXyz57T-4",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-1"
},
{
"id": "z8z-17g71uec0qXyz57T-5",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-4",
"label": "*"
},
{
"id": "z8z-17g71uec0qXyz57T-6",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-4",
"label": "JobTitle : nvarchar(50)"
},
{
"id": "z8z-17g71uec0qXyz57T-1",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-1"
},
{
"id": "z8z-17g71uec0qXyz57T-2",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-1",
"label": "* D"
},
{
"id": "z8z-17g71uec0qXyz57T-3",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-1",
"label": "IsActive : bool"
},
{
"id": "z8z-17g71uec0qXyz57T-7",
"type": "edge",
"parent": "9bh975Iajj_f-3G999F8-1",
"source": "9bh975Iajj_f-3G999F8-2",
"target": "aBztl0BIyrloTKxhFtse-3"
},
{
"id": "nfNeyiAOdsavxUQbhxos-28",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-1"
},
{
"id": "nfNeyiAOdsavxUQbhxos-29",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-28",
"label": "*"
},
{
"id": "nfNeyiAOdsavxUQbhxos-30",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-28",
"label": "CreatedAt : DateTime()"
},
{
"id": "nfNeyiAOdsavxUQbhxos-31",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-1"
},
{
"id": "nfNeyiAOdsavxUQbhxos-32",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-31"
},
{
"id": "nfNeyiAOdsavxUQbhxos-33",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-31",
"label": "UpdatedAt : DateTime()"
},
{
"id": "9bh975Iajj_f-3G999F8-14",
"type": "node",
"parent": "1",
"label": "Role"
},
{
"id": "9bh975Iajj_f-3G999F8-15",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-14"
},
{
"id": "9bh975Iajj_f-3G999F8-16",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-15",
"label": "PK"
},
{
"id": "9bh975Iajj_f-3G999F8-17",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-15",
"label": "Id GUID"
},
{
"id": "9bh975Iajj_f-3G999F8-18",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-14"
},
{
"id": "9bh975Iajj_f-3G999F8-19",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-18",
"label": "*"
},
{
"id": "9bh975Iajj_f-3G999F8-20",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-18",
"label": "Name : nvarchar(50)"
},
{
"id": "9bh975Iajj_f-3G999F8-21",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-14"
},
{
"id": "9bh975Iajj_f-3G999F8-22",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-21"
},
{
"id": "9bh975Iajj_f-3G999F8-23",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-21",
"label": "Description : nvarchar(max)"
},
{
"id": "9bh975Iajj_f-3G999F8-24",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-14"
},
{
"id": "9bh975Iajj_f-3G999F8-25",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-24",
"label": "* D"
},
{
"id": "9bh975Iajj_f-3G999F8-26",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-24",
"label": "IsActive : bool"
},
{
"id": "nfNeyiAOdsavxUQbhxos-37",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-14"
},
{
"id": "nfNeyiAOdsavxUQbhxos-38",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-37",
"label": "*"
},
{
"id": "nfNeyiAOdsavxUQbhxos-39",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-37",
"label": "CreatedAt : DateTime()"
},
{
"id": "nfNeyiAOdsavxUQbhxos-34",
"type": "node",
"parent": "9bh975Iajj_f-3G999F8-14"
},
{
"id": "nfNeyiAOdsavxUQbhxos-35",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-34"
},
{
"id": "nfNeyiAOdsavxUQbhxos-36",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-34",
"label": "UpdatedAt : DateTime()"
},
{
"id": "z8z-17g71uec0qXyz57T-10",
"type": "node",
"parent": "1",
"label": "Account"
},
{
"id": "z8z-17g71uec0qXyz57T-11",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-10"
},
{
"id": "z8z-17g71uec0qXyz57T-12",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-11",
"label": "PK"
},
{
"id": "z8z-17g71uec0qXyz57T-13",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-11",
"label": "Id GUID"
},
{
"id": "z8z-17g71uec0qXyz57T-23",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-10"
},
{
"id": "z8z-17g71uec0qXyz57T-24",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-23",
"label": "FK"
},
{
"id": "z8z-17g71uec0qXyz57T-25",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-23",
"label": "EmployeeId GUID"
},
{
"id": "z8z-17g71uec0qXyz57T-42",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-10"
},
{
"id": "z8z-17g71uec0qXyz57T-43",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-42",
"label": "FK"
},
{
"id": "z8z-17g71uec0qXyz57T-44",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-42",
"label": "RoleId GUID"
},
{
"id": "z8z-17g71uec0qXyz57T-14",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-10"
},
{
"id": "z8z-17g71uec0qXyz57T-15",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-14",
"label": "* U"
},
{
"id": "z8z-17g71uec0qXyz57T-16",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-14",
"label": "Username: nvarchar(50)"
},
{
"id": "z8z-17g71uec0qXyz57T-17",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-10"
},
{
"id": "z8z-17g71uec0qXyz57T-18",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-17",
"label": "*"
},
{
"id": "z8z-17g71uec0qXyz57T-19",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-17",
"label": "Password : hash"
},
{
"id": "z8z-17g71uec0qXyz57T-20",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-10"
},
{
"id": "z8z-17g71uec0qXyz57T-21",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-20",
"label": "* D"
},
{
"id": "z8z-17g71uec0qXyz57T-22",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-20",
"label": "IsActive : bool"
},
{
"id": "z8z-17g71uec0qXyz57T-45",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-10"
},
{
"id": "z8z-17g71uec0qXyz57T-46",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-45"
},
{
"id": "z8z-17g71uec0qXyz57T-47",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-45",
"label": "LastLoginAt : DateTime()"
},
{
"id": "nfNeyiAOdsavxUQbhxos-40",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-10"
},
{
"id": "nfNeyiAOdsavxUQbhxos-41",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-40",
"label": "*"
},
{
"id": "nfNeyiAOdsavxUQbhxos-42",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-40",
"label": "CreatedAt : DateTime()"
},
{
"id": "nfNeyiAOdsavxUQbhxos-43",
"type": "node",
"parent": "z8z-17g71uec0qXyz57T-10"
},
{
"id": "nfNeyiAOdsavxUQbhxos-44",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-43"
},
{
"id": "nfNeyiAOdsavxUQbhxos-45",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-43",
"label": "UpdatedAt : DateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-63",
"type": "edge",
"parent": "1",
"source": "xgTkMUlbhj2w8nvYlmg2-2",
"target": "Jt6paovLrLvcBHmawJKR-36"
},
{
"id": "z8z-17g71uec0qXyz57T-48",
"type": "edge",
"parent": "1",
"source": "9bh975Iajj_f-3G999F8-15",
"target": "z8z-17g71uec0qXyz57T-42"
},
{
"id": "z8z-17g71uec0qXyz57T-49",
"type": "edge",
"parent": "1",
"source": "9bh975Iajj_f-3G999F8-2",
"target": "z8z-17g71uec0qXyz57T-23"
},
{
"id": "LyIj3it893yShO42AiGY-1",
"type": "node",
"parent": "1",
"label": "Department"
},
{
"id": "LyIj3it893yShO42AiGY-2",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-1"
},
{
"id": "LyIj3it893yShO42AiGY-3",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-2",
"label": "PK"
},
{
"id": "LyIj3it893yShO42AiGY-4",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-2",
"label": "Id GUID"
},
{
"id": "LyIj3it893yShO42AiGY-17",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-1"
},
{
"id": "LyIj3it893yShO42AiGY-18",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-17",
"label": "FK"
},
{
"id": "LyIj3it893yShO42AiGY-19",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-17",
"label": "ParentDepartmentId GUID"
},
{
"id": "LyIj3it893yShO42AiGY-5",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-1"
},
{
"id": "LyIj3it893yShO42AiGY-6",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-5",
"label": "*"
},
{
"id": "LyIj3it893yShO42AiGY-7",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-5",
"label": "Code : nvarchar (50)"
},
{
"id": "LyIj3it893yShO42AiGY-8",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-1"
},
{
"id": "LyIj3it893yShO42AiGY-9",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-8",
"label": "*"
},
{
"id": "LyIj3it893yShO42AiGY-10",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-8",
"label": "Name : nvarchar(50)"
},
{
"id": "LyIj3it893yShO42AiGY-11",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-1"
},
{
"id": "LyIj3it893yShO42AiGY-12",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-11"
},
{
"id": "LyIj3it893yShO42AiGY-13",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-11",
"label": "Description : nvarchar(max)"
},
{
"id": "z8z-17g71uec0qXyz57T-9",
"type": "edge",
"parent": "LyIj3it893yShO42AiGY-1",
"source": "LyIj3it893yShO42AiGY-2",
"target": "LyIj3it893yShO42AiGY-17"
},
{
"id": "nfNeyiAOdsavxUQbhxos-22",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-1"
},
{
"id": "nfNeyiAOdsavxUQbhxos-23",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-22",
"label": "*"
},
{
"id": "nfNeyiAOdsavxUQbhxos-24",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-22",
"label": "CreatedAt : DateTime()"
},
{
"id": "nfNeyiAOdsavxUQbhxos-25",
"type": "node",
"parent": "LyIj3it893yShO42AiGY-1"
},
{
"id": "nfNeyiAOdsavxUQbhxos-26",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-25"
},
{
"id": "nfNeyiAOdsavxUQbhxos-27",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-25",
"label": "UpdatedAt : DateTime()"
},
{
"id": "xgTkMUlbhj2w8nvYlmg2-1",
"type": "node",
"parent": "1",
"label": "Question"
},
{
"id": "xgTkMUlbhj2w8nvYlmg2-2",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-1"
},
{
"id": "xgTkMUlbhj2w8nvYlmg2-3",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-2",
"label": "PK"
},
{
"id": "xgTkMUlbhj2w8nvYlmg2-4",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-2",
"label": "Id GUID"
},
{
"id": "Jt6paovLrLvcBHmawJKR-7",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-1"
},
{
"id": "Jt6paovLrLvcBHmawJKR-8",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-7",
"label": "FK"
},
{
"id": "Jt6paovLrLvcBHmawJKR-9",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-7",
"label": "QuestionBankId GUID"
},
{
"id": "xgTkMUlbhj2w8nvYlmg2-5",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-1"
},
{
"id": "xgTkMUlbhj2w8nvYlmg2-6",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-5",
"label": "*"
},
{
"id": "xgTkMUlbhj2w8nvYlmg2-7",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-5",
"label": "QuestionText : nvarchar ()"
},
{
"id": "xgTkMUlbhj2w8nvYlmg2-8",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-1"
},
{
"id": "xgTkMUlbhj2w8nvYlmg2-9",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-8",
"label": "*"
},
{
"id": "xgTkMUlbhj2w8nvYlmg2-10",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-8",
"label": "QuestionType : enum"
},
{
"id": "xgTkMUlbhj2w8nvYlmg2-11",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-1"
},
{
"id": "xgTkMUlbhj2w8nvYlmg2-12",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-11",
"label": "*"
},
{
"id": "xgTkMUlbhj2w8nvYlmg2-13",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-11",
"label": "Weight : decimal"
},
{
"id": "Jt6paovLrLvcBHmawJKR-17",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-1"
},
{
"id": "Jt6paovLrLvcBHmawJKR-18",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-17",
"label": "* c"
},
{
"id": "Jt6paovLrLvcBHmawJKR-19",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-17",
"label": "SequenceNumber : int"
},
{
"id": "Jt6paovLrLvcBHmawJKR-20",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-1"
},
{
"id": "Jt6paovLrLvcBHmawJKR-21",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-20",
"label": "* D"
},
{
"id": "Jt6paovLrLvcBHmawJKR-22",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-20",
"label": "IsRequired : bool"
},
{
"id": "Jt6paovLrLvcBHmawJKR-23",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-1"
},
{
"id": "Jt6paovLrLvcBHmawJKR-24",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-23",
"label": "* D"
},
{
"id": "Jt6paovLrLvcBHmawJKR-25",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-23",
"label": "IsActive : bool"
},
{
"id": "Jt6paovLrLvcBHmawJKR-26",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-1"
},
{
"id": "Jt6paovLrLvcBHmawJKR-27",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-26",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-28",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-26",
"label": "CreatedAt : DateTime()"
},
{
"id": "nfNeyiAOdsavxUQbhxos-7",
"type": "node",
"parent": "xgTkMUlbhj2w8nvYlmg2-1"
},
{
"id": "nfNeyiAOdsavxUQbhxos-8",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-7"
},
{
"id": "nfNeyiAOdsavxUQbhxos-9",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-7",
"label": "UpdatedAt : DateTime()"
},
{
"id": "Oy1qiGlo1KGQVJ0b-qb3-1",
"type": "node",
"parent": "1",
"label": "BatchApplications"
},
{
"id": "Oy1qiGlo1KGQVJ0b-qb3-2",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-1"
},
{
"id": "Oy1qiGlo1KGQVJ0b-qb3-3",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-2",
"label": "PK"
},
{
"id": "Oy1qiGlo1KGQVJ0b-qb3-4",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-2",
"label": "Id : guid"
},
{
"id": "Oy1qiGlo1KGQVJ0b-qb3-8",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-1"
},
{
"id": "Oy1qiGlo1KGQVJ0b-qb3-9",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-8",
"label": "FK"
},
{
"id": "Oy1qiGlo1KGQVJ0b-qb3-10",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-8",
"label": "CandidateId : guid"
},
{
"id": "Oy1qiGlo1KGQVJ0b-qb3-5",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-1"
},
{
"id": "Oy1qiGlo1KGQVJ0b-qb3-6",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-5",
"label": "FK"
},
{
"id": "Oy1qiGlo1KGQVJ0b-qb3-7",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-5",
"label": "RecruitmentBatchId : Guid"
},
{
"id": "Oy1qiGlo1KGQVJ0b-qb3-11",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-1"
},
{
"id": "Oy1qiGlo1KGQVJ0b-qb3-12",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-11",
"label": "* U"
},
{
"id": "Oy1qiGlo1KGQVJ0b-qb3-13",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-11",
"label": "CodeApplication : nvarchar(50)"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-41",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-1"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-42",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-41",
"label": "*"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-43",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-41",
"label": "AppliedAt : dateTime()"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-29",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-1"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-30",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-29",
"label": "*"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-31",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-29",
"label": "FinalResult : varchar(30)"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-38",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-1"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-39",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-38",
"label": "*"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-40",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-38",
"label": "Status : enum"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-32",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-1"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-33",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-32"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-34",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-32",
"label": "Note : nvarvhar(100)"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-26",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-1"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-27",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-26",
"label": "*"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-28",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-26",
"label": "CreatedAt : dateTime()"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-44",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-1"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-45",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-44"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-46",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-44",
"label": "UpdatedAt : dateTime()"
},
{
"id": "ADCSfhaxFZBl8uSsRkzj-1",
"type": "node",
"parent": "Oy1qiGlo1KGQVJ0b-qb3-1"
},
{
"id": "ADCSfhaxFZBl8uSsRkzj-2",
"type": "node",
"parent": "ADCSfhaxFZBl8uSsRkzj-1"
},
{
"id": "ADCSfhaxFZBl8uSsRkzj-3",
"type": "node",
"parent": "ADCSfhaxFZBl8uSsRkzj-1",
"label": "FileOfferingLetter : nvarchar(100)"
},
{
"id": "z8z-17g71uec0qXyz57T-8",
"type": "edge",
"parent": "1",
"source": "LyIj3it893yShO42AiGY-2",
"target": "aBztl0BIyrloTKxhFtse-10"
},
{
"id": "Jt6paovLrLvcBHmawJKR-32",
"type": "node",
"parent": "1",
"label": "QuestionOptions"
},
{
"id": "Jt6paovLrLvcBHmawJKR-33",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-32"
},
{
"id": "Jt6paovLrLvcBHmawJKR-34",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-33",
"label": "PK"
},
{
"id": "Jt6paovLrLvcBHmawJKR-35",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-33",
"label": "Id GUID"
},
{
"id": "Jt6paovLrLvcBHmawJKR-36",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-32"
},
{
"id": "Jt6paovLrLvcBHmawJKR-37",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-36",
"label": "FK"
},
{
"id": "Jt6paovLrLvcBHmawJKR-38",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-36",
"label": "QuestionId GUID"
},
{
"id": "Jt6paovLrLvcBHmawJKR-39",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-32"
},
{
"id": "Jt6paovLrLvcBHmawJKR-40",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-39",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-41",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-39",
"label": "OptionLabel : nvarchar(10)"
},
{
"id": "Jt6paovLrLvcBHmawJKR-42",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-32"
},
{
"id": "Jt6paovLrLvcBHmawJKR-43",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-42",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-44",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-42",
"label": "OptionText : nvarchar()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-45",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-32"
},
{
"id": "Jt6paovLrLvcBHmawJKR-46",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-45",
"label": "* C"
},
{
"id": "Jt6paovLrLvcBHmawJKR-47",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-45",
"label": "Score : decimal"
},
{
"id": "Jt6paovLrLvcBHmawJKR-48",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-32"
},
{
"id": "Jt6paovLrLvcBHmawJKR-49",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-48",
"label": "* C"
},
{
"id": "Jt6paovLrLvcBHmawJKR-50",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-48",
"label": "SequenceNumber: int"
},
{
"id": "Jt6paovLrLvcBHmawJKR-51",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-32"
},
{
"id": "Jt6paovLrLvcBHmawJKR-52",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-51",
"label": "* D"
},
{
"id": "Jt6paovLrLvcBHmawJKR-53",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-51",
"label": "IsActive : bool"
},
{
"id": "nfNeyiAOdsavxUQbhxos-10",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-32"
},
{
"id": "nfNeyiAOdsavxUQbhxos-11",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-10",
"label": "*"
},
{
"id": "nfNeyiAOdsavxUQbhxos-12",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-10",
"label": "CreatedAt : DateTime()"
},
{
"id": "nfNeyiAOdsavxUQbhxos-13",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-32"
},
{
"id": "nfNeyiAOdsavxUQbhxos-14",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-13"
},
{
"id": "nfNeyiAOdsavxUQbhxos-15",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-13",
"label": "UpdatedAt : DateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-96",
"type": "node",
"parent": "1",
"label": "BatchStage"
},
{
"id": "Jt6paovLrLvcBHmawJKR-97",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-96"
},
{
"id": "Jt6paovLrLvcBHmawJKR-98",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-97",
"label": "PK"
},
{
"id": "Jt6paovLrLvcBHmawJKR-99",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-97",
"label": "Id GUID"
},
{
"id": "nfNeyiAOdsavxUQbhxos-1",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-96"
},
{
"id": "nfNeyiAOdsavxUQbhxos-2",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-1",
"label": "PK,FK"
},
{
"id": "nfNeyiAOdsavxUQbhxos-3",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-1",
"label": "NextStageId GUID"
},
{
"id": "Jt6paovLrLvcBHmawJKR-100",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-96"
},
{
"id": "Jt6paovLrLvcBHmawJKR-101",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-100",
"label": "FK"
},
{
"id": "Jt6paovLrLvcBHmawJKR-102",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-100",
"label": "RecruitmentBatchId GUID"
},
{
"id": "Jt6paovLrLvcBHmawJKR-123",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-96"
},
{
"id": "Jt6paovLrLvcBHmawJKR-124",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-123",
"label": "FK"
},
{
"id": "Jt6paovLrLvcBHmawJKR-125",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-123",
"label": "StageTypeId GUID"
},
{
"id": "Jt6paovLrLvcBHmawJKR-103",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-96"
},
{
"id": "Jt6paovLrLvcBHmawJKR-104",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-103",
"label": "* U"
},
{
"id": "Jt6paovLrLvcBHmawJKR-105",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-103",
"label": "StageName : nvarchar(50)"
},
{
"id": "Jt6paovLrLvcBHmawJKR-109",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-96"
},
{
"id": "Jt6paovLrLvcBHmawJKR-110",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-109",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-111",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-109",
"label": "IsRequired : bool"
},
{
"id": "Jt6paovLrLvcBHmawJKR-112",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-96"
},
{
"id": "Jt6paovLrLvcBHmawJKR-113",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-112",
"label": "* C"
},
{
"id": "Jt6paovLrLvcBHmawJKR-114",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-112",
"label": "PassingScore : decimal"
},
{
"id": "Jt6paovLrLvcBHmawJKR-115",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-96"
},
{
"id": "Jt6paovLrLvcBHmawJKR-116",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-115",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-117",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-115",
"label": "StartDate : dateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-129",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-96"
},
{
"id": "Jt6paovLrLvcBHmawJKR-130",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-129",
"label": "* C"
},
{
"id": "Jt6paovLrLvcBHmawJKR-131",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-129",
"label": "EndDate : dateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-132",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-96"
},
{
"id": "Jt6paovLrLvcBHmawJKR-133",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-132",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-134",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-132",
"label": "Status : enum"
},
{
"id": "Jt6paovLrLvcBHmawJKR-135",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-96"
},
{
"id": "Jt6paovLrLvcBHmawJKR-136",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-135",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-137",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-135",
"label": "CreatedAt : dateTime()"
},
{
"id": "nfNeyiAOdsavxUQbhxos-19",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-96"
},
{
"id": "nfNeyiAOdsavxUQbhxos-20",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-19"
},
{
"id": "nfNeyiAOdsavxUQbhxos-21",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-19",
"label": "UpdatedAt : dateTime()"
},
{
"id": "3ZIVxYjwdYr_oaVrmmr2-1",
"type": "node",
"parent": "1",
"label": "Candidates"
},
{
"id": "3ZIVxYjwdYr_oaVrmmr2-2",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-1"
},
{
"id": "3ZIVxYjwdYr_oaVrmmr2-3",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-2",
"label": "PK"
},
{
"id": "3ZIVxYjwdYr_oaVrmmr2-4",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-2",
"label": "Id : Guid"
},
{
"id": "3ZIVxYjwdYr_oaVrmmr2-5",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-1"
},
{
"id": "3ZIVxYjwdYr_oaVrmmr2-6",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-5",
"label": "*"
},
{
"id": "3ZIVxYjwdYr_oaVrmmr2-7",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-5",
"label": "FullName : nvarchar(50)"
},
{
"id": "3ZIVxYjwdYr_oaVrmmr2-8",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-1"
},
{
"id": "3ZIVxYjwdYr_oaVrmmr2-9",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-8",
"label": "* U"
},
{
"id": "3ZIVxYjwdYr_oaVrmmr2-10",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-8",
"label": "Email : nvarchar(50)"
},
{
"id": "3ZIVxYjwdYr_oaVrmmr2-11",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-1"
},
{
"id": "3ZIVxYjwdYr_oaVrmmr2-12",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-11",
"label": "* U"
},
{
"id": "3ZIVxYjwdYr_oaVrmmr2-13",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-11",
"label": "PhoneNumber : int"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-52",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-1"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-53",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-52",
"label": "*"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-54",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-52",
"label": "BirthDate : date()"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-4",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-1"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-5",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-4"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-6",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-4",
"label": "CV : nvarchar(100)"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-7",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-1"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-8",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-7"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-9",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-7",
"label": "Portfolio : nvarchar(100)"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-10",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-1"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-11",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-10",
"label": "*"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-12",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-10",
"label": "Education : nvarchar(50)"
},
{
"id": "dW8n03HlUMNgjClEFn8q-1",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-1"
},
{
"id": "dW8n03HlUMNgjClEFn8q-2",
"type": "node",
"parent": "dW8n03HlUMNgjClEFn8q-1",
"label": "*"
},
{
"id": "dW8n03HlUMNgjClEFn8q-3",
"type": "node",
"parent": "dW8n03HlUMNgjClEFn8q-1",
"label": "Institution : nvarchar(50)"
},
{
"id": "dW8n03HlUMNgjClEFn8q-4",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-1"
},
{
"id": "dW8n03HlUMNgjClEFn8q-5",
"type": "node",
"parent": "dW8n03HlUMNgjClEFn8q-4",
"label": "*"
},
{
"id": "dW8n03HlUMNgjClEFn8q-6",
"type": "node",
"parent": "dW8n03HlUMNgjClEFn8q-4",
"label": "Address : nvarchar(50)"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-13",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-1"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-14",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-13",
"label": "* C"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-15",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-13",
"label": "GPA : nvarchar(50)"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-19",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-1"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-20",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-19",
"label": "*"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-21",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-19",
"label": "CreatedAt : DateTime()"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-22",
"type": "node",
"parent": "3ZIVxYjwdYr_oaVrmmr2-1"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-23",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-22"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-24",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-22",
"label": "UpdatedAt : DateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-118",
"type": "edge",
"parent": "1",
"source": "Jt6paovLrLvcBHmawJKR-229",
"target": "Jt6paovLrLvcBHmawJKR-100"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-21",
"type": "node",
"parent": "1",
"label": "CandidateAnswer"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-22",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-21"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-23",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-22",
"label": "PK"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-24",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-22",
"label": "Id GUID"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-34",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-21"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-35",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-34",
"label": "FK"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-36",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-34",
"label": "ApplicationStageId GUID"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-25",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-21"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-26",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-25",
"label": "*"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-27",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-25",
"label": "QuestionText : nvarchar(100)"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-28",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-21"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-29",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-28",
"label": "*"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-30",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-28",
"label": "QuestionOptionText : nvarchar(100)"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-31",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-21"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-32",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-31",
"label": "*"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-33",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-31",
"label": "Score&nbsp; : decimal"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-37",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-21"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-38",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-37",
"label": "*"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-39",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-37",
"label": "CreatedAt : dateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-138",
"type": "node",
"parent": "1",
"label": "StageType"
},
{
"id": "Jt6paovLrLvcBHmawJKR-139",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-138"
},
{
"id": "Jt6paovLrLvcBHmawJKR-140",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-139",
"label": "PK"
},
{
"id": "Jt6paovLrLvcBHmawJKR-141",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-139",
"label": "Id GUID"
},
{
"id": "Jt6paovLrLvcBHmawJKR-145",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-138"
},
{
"id": "Jt6paovLrLvcBHmawJKR-146",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-145",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-147",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-145",
"label": "Name : varchar(30)"
},
{
"id": "Jt6paovLrLvcBHmawJKR-148",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-138"
},
{
"id": "Jt6paovLrLvcBHmawJKR-149",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-148",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-150",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-148",
"label": "Description : nvarchar(100)"
},
{
"id": "Jt6paovLrLvcBHmawJKR-151",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-138"
},
{
"id": "Jt6paovLrLvcBHmawJKR-152",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-151",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-153",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-151",
"label": "StageCategory : nvarchar(100)"
},
{
"id": "Jt6paovLrLvcBHmawJKR-154",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-138"
},
{
"id": "Jt6paovLrLvcBHmawJKR-155",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-154",
"label": "* D"
},
{
"id": "Jt6paovLrLvcBHmawJKR-156",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-154",
"label": "IsActive : bool"
},
{
"id": "Jt6paovLrLvcBHmawJKR-157",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-138"
},
{
"id": "Jt6paovLrLvcBHmawJKR-158",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-157",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-159",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-157",
"label": "CreatedAt : dateTime()"
},
{
"id": "nfNeyiAOdsavxUQbhxos-16",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-138"
},
{
"id": "nfNeyiAOdsavxUQbhxos-17",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-16"
},
{
"id": "nfNeyiAOdsavxUQbhxos-18",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-16",
"label": "UpdatedAt : dateTime()"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-40",
"type": "node",
"parent": "1",
"label": "InterviewSession"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-41",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-40"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-42",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-41",
"label": "PK"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-43",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-41",
"label": "Id GUID"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-53",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-40"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-54",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-53",
"label": "FK"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-55",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-53",
"label": "ApplicationStageId GUID"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-44",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-40"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-45",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-44",
"label": "*"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-46",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-44",
"label": "SessionName : varchar(50)"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-47",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-40"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-48",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-47",
"label": "*"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-49",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-47",
"label": "ScheduledStart : DateTime()"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-50",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-40"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-51",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-50",
"label": "*"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-52",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-50",
"label": "ScheduledEnd : DateTime()"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-56",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-40"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-57",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-56",
"label": "*"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-58",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-56",
"label": "IsOnline : bool"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-59",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-40"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-60",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-59"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-61",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-59",
"label": "Location : nvarchar(100)"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-62",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-40"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-63",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-62"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-64",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-62",
"label": "MeetingUrl : nvarchar(100)"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-65",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-40"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-66",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-65",
"label": "*"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-67",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-65",
"label": "Status&nbsp; : varchar(30)"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-102",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-40"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-103",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-102"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-104",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-102",
"label": "UpdatedAt : dateTime()"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-96",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-40"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-97",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-96",
"label": "*"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-98",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-96",
"label": "CreatedBy : guid"
},
{
"id": "nfNeyiAOdsavxUQbhxos-85",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-40"
},
{
"id": "nfNeyiAOdsavxUQbhxos-86",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-85"
},
{
"id": "nfNeyiAOdsavxUQbhxos-87",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-85",
"label": "FileSubmission : nvarchar(100)"
},
{
"id": "VNNj8byyzlCblofe_NYs-1",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-40"
},
{
"id": "VNNj8byyzlCblofe_NYs-2",
"type": "node",
"parent": "VNNj8byyzlCblofe_NYs-1"
},
{
"id": "VNNj8byyzlCblofe_NYs-3",
"type": "node",
"parent": "VNNj8byyzlCblofe_NYs-1",
"label": "EndedSubmissionAt : dateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-160",
"type": "edge",
"parent": "1",
"source": "Jt6paovLrLvcBHmawJKR-139",
"target": "Jt6paovLrLvcBHmawJKR-123"
},
{
"id": "Jt6paovLrLvcBHmawJKR-161",
"type": "node",
"parent": "1",
"label": "ApplicationStage"
},
{
"id": "Jt6paovLrLvcBHmawJKR-162",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-161"
},
{
"id": "Jt6paovLrLvcBHmawJKR-163",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-162",
"label": "PK"
},
{
"id": "Jt6paovLrLvcBHmawJKR-164",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-162",
"label": "Id GUID"
},
{
"id": "Jt6paovLrLvcBHmawJKR-271",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-161"
},
{
"id": "Jt6paovLrLvcBHmawJKR-272",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-271",
"label": "FK"
},
{
"id": "Jt6paovLrLvcBHmawJKR-273",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-271",
"label": "BatchApplicationId GUID"
},
{
"id": "Jt6paovLrLvcBHmawJKR-262",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-161"
},
{
"id": "Jt6paovLrLvcBHmawJKR-263",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-262",
"label": "FK"
},
{
"id": "Jt6paovLrLvcBHmawJKR-264",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-262",
"label": "BatchStageId GUID"
},
{
"id": "Jt6paovLrLvcBHmawJKR-165",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-161"
},
{
"id": "Jt6paovLrLvcBHmawJKR-166",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-165",
"label": "* D"
},
{
"id": "Jt6paovLrLvcBHmawJKR-167",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-165",
"label": "Status : enum"
},
{
"id": "Jt6paovLrLvcBHmawJKR-168",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-161"
},
{
"id": "Jt6paovLrLvcBHmawJKR-169",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-168"
},
{
"id": "Jt6paovLrLvcBHmawJKR-170",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-168",
"label": "TotalScore : decimal"
},
{
"id": "Jt6paovLrLvcBHmawJKR-174",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-161"
},
{
"id": "Jt6paovLrLvcBHmawJKR-175",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-174",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-176",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-174",
"label": "StartedAt : dateTime()"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-38",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-161"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-39",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-38",
"label": "*"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-40",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-38",
"label": "EndedAt : dateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-177",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-161"
},
{
"id": "Jt6paovLrLvcBHmawJKR-178",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-177"
},
{
"id": "Jt6paovLrLvcBHmawJKR-179",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-177",
"label": "CompletedAt : dateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-274",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-161"
},
{
"id": "Jt6paovLrLvcBHmawJKR-275",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-274"
},
{
"id": "Jt6paovLrLvcBHmawJKR-276",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-274",
"label": "Notes : nvarchar(100)"
},
{
"id": "vu0sGOmyCkF3KWFEINvx-6",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-161"
},
{
"id": "vu0sGOmyCkF3KWFEINvx-7",
"type": "node",
"parent": "vu0sGOmyCkF3KWFEINvx-6",
"label": "* D"
},
{
"id": "vu0sGOmyCkF3KWFEINvx-8",
"type": "node",
"parent": "vu0sGOmyCkF3KWFEINvx-6",
"label": "IsAttempts : bool"
},
{
"id": "Jt6paovLrLvcBHmawJKR-277",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-161"
},
{
"id": "Jt6paovLrLvcBHmawJKR-278",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-277",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-279",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-277",
"label": "CreatedAt : dateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-280",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-161"
},
{
"id": "Jt6paovLrLvcBHmawJKR-281",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-280"
},
{
"id": "Jt6paovLrLvcBHmawJKR-282",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-280",
"label": "UpdatedAt : dateTime()"
},
{
"id": "9mgvTgr9zOqUdC9oKu9_-1",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-161"
},
{
"id": "9mgvTgr9zOqUdC9oKu9_-2",
"type": "node",
"parent": "9mgvTgr9zOqUdC9oKu9_-1"
},
{
"id": "9mgvTgr9zOqUdC9oKu9_-3",
"type": "node",
"parent": "9mgvTgr9zOqUdC9oKu9_-1",
"label": "DurationTest: Time()"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-47",
"type": "edge",
"parent": "1",
"source": "3ZIVxYjwdYr_oaVrmmr2-2",
"target": "Oy1qiGlo1KGQVJ0b-qb3-8"
},
{
"id": "Jt6paovLrLvcBHmawJKR-228",
"type": "node",
"parent": "1",
"label": "RecruitmentBatch"
},
{
"id": "Jt6paovLrLvcBHmawJKR-229",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-228"
},
{
"id": "Jt6paovLrLvcBHmawJKR-230",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-229",
"label": "PK"
},
{
"id": "Jt6paovLrLvcBHmawJKR-231",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-229",
"label": "Id GUID"
},
{
"id": "nfNeyiAOdsavxUQbhxos-51",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-228"
},
{
"id": "nfNeyiAOdsavxUQbhxos-52",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-51",
"label": "FK"
},
{
"id": "nfNeyiAOdsavxUQbhxos-53",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-51",
"label": "DepartmentId GUID"
},
{
"id": "Jt6paovLrLvcBHmawJKR-232",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-228"
},
{
"id": "Jt6paovLrLvcBHmawJKR-233",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-232",
"label": "* U"
},
{
"id": "Jt6paovLrLvcBHmawJKR-234",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-232",
"label": "BatchCode : nvarchar(50)"
},
{
"id": "Jt6paovLrLvcBHmawJKR-235",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-228"
},
{
"id": "Jt6paovLrLvcBHmawJKR-236",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-235",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-237",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-235",
"label": "Name : nvarchar(50)"
},
{
"id": "Jt6paovLrLvcBHmawJKR-238",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-228"
},
{
"id": "Jt6paovLrLvcBHmawJKR-239",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-238",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-240",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-238",
"label": "Description : nvarchar(max)"
},
{
"id": "Jt6paovLrLvcBHmawJKR-241",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-228"
},
{
"id": "Jt6paovLrLvcBHmawJKR-242",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-241",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-243",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-241",
"label": "StartDate : DateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-244",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-228"
},
{
"id": "Jt6paovLrLvcBHmawJKR-245",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-244",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-246",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-244",
"label": "EndDate : DateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-247",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-228"
},
{
"id": "Jt6paovLrLvcBHmawJKR-248",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-247",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-249",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-247",
"label": "Status : enum"
},
{
"id": "Jt6paovLrLvcBHmawJKR-250",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-228"
},
{
"id": "Jt6paovLrLvcBHmawJKR-251",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-250",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-252",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-250",
"label": "CreatedBy : guid"
},
{
"id": "Jt6paovLrLvcBHmawJKR-253",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-228"
},
{
"id": "Jt6paovLrLvcBHmawJKR-254",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-253",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-255",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-253",
"label": "CreatedAt : dateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-256",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-228"
},
{
"id": "Jt6paovLrLvcBHmawJKR-257",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-256"
},
{
"id": "Jt6paovLrLvcBHmawJKR-258",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-256",
"label": "UpdatedAt : dateTime()"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-105",
"type": "node",
"parent": "1",
"label": "StageEmployee"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-106",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-105"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-107",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-106",
"label": "PK"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-108",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-106",
"label": "Id GUID"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-109",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-105"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-110",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-109",
"label": "FK"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-111",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-109",
"label": "BatchStageId GUID"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-142",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-105"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-143",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-142",
"label": "FK"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-144",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-142",
"label": "EmployeeId GUID"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-112",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-105"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-113",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-112",
"label": "*"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-114",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-112",
"label": "Status : enum"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-67",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-105"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-68",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-67",
"label": "* D"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-69",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-67",
"label": "IsActive : bool"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-64",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-105"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-65",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-64"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-66",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-64",
"label": "Note : varchar(100)"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-58",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-105"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-59",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-58",
"label": "*"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-60",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-58",
"label": "CreatedAt : DateTime()"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-61",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-105"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-62",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-61"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-63",
"type": "node",
"parent": "Y3jVaBpkGW-p0XrtYrB6-61",
"label": "UpdateAt : DateTime()"
},
{
"id": "Y3jVaBpkGW-p0XrtYrB6-51",
"type": "edge",
"parent": "1",
"source": "Jt6paovLrLvcBHmawJKR-229",
"target": "Oy1qiGlo1KGQVJ0b-qb3-5"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-148",
"type": "node",
"parent": "1",
"label": "SessionInterview"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-149",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-148"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-150",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-149",
"label": "PK"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-151",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-149",
"label": "Id GUID"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-155",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-148"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-156",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-155",
"label": "PK,FK"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-157",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-155",
"label": "StageEmployeeId GUID"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-152",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-148"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-153",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-152",
"label": "PK,FK"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-154",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-152",
"label": "InterviewSessionId GUID"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-158",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-148"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-159",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-158",
"label": "*"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-160",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-158",
"label": "Status : enum"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-161",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-148"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-162",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-161",
"label": "*"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-163",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-161",
"label": "CreatedAt : DateTime()"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-164",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-148"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-165",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-164"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-166",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-164",
"label": "UpdatedAt&nbsp; : DateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-283",
"type": "edge",
"parent": "1",
"source": "Oy1qiGlo1KGQVJ0b-qb3-2",
"target": "Jt6paovLrLvcBHmawJKR-271"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-180",
"type": "node",
"parent": "1",
"label": "InterviewAssessment"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-181",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-180"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-182",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-181",
"label": "PK"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-183",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-181",
"label": "Id GUID"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-193",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-180"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-194",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-193",
"label": "FK"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-195",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-193",
"label": "SessionInterviewId GUID"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-184",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-180"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-185",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-184"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-186",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-184",
"label": "Notes : nvarchar(max)"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-187",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-180"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-188",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-187",
"label": "*&nbsp;"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-189",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-187",
"label": "Recommendation: enum"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-190",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-180"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-191",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-190",
"label": "* C"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-192",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-190",
"label": "Score: decimal"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-196",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-180"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-197",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-196",
"label": "*"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-198",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-196",
"label": "CreatedAt : DateTime()"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-199",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-180"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-200",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-199"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-201",
"type": "node",
"parent": "Ni1uZt9kZrgzDhyM-i3v-199",
"label": "UpdatedAt : DatetTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-284",
"type": "edge",
"parent": "1",
"source": "Jt6paovLrLvcBHmawJKR-97",
"target": "Jt6paovLrLvcBHmawJKR-262"
},
{
"id": "Jt6paovLrLvcBHmawJKR-285",
"type": "edge",
"parent": "1",
"source": "Jt6paovLrLvcBHmawJKR-162",
"target": "Ni1uZt9kZrgzDhyM-i3v-34"
},
{
"id": "Jt6paovLrLvcBHmawJKR-286",
"type": "edge",
"parent": "1",
"source": "Jt6paovLrLvcBHmawJKR-162",
"target": "Ni1uZt9kZrgzDhyM-i3v-53"
},
{
"id": "Jt6paovLrLvcBHmawJKR-287",
"type": "node",
"parent": "1",
"label": "RescheduleRequest"
},
{
"id": "Jt6paovLrLvcBHmawJKR-291",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-287"
},
{
"id": "Jt6paovLrLvcBHmawJKR-292",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-291",
"label": "PK"
},
{
"id": "Jt6paovLrLvcBHmawJKR-293",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-291",
"label": "Id GUID"
},
{
"id": "Jt6paovLrLvcBHmawJKR-288",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-287"
},
{
"id": "Jt6paovLrLvcBHmawJKR-289",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-288",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-290",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-288",
"label": "InterviewSessionId GUID"
},
{
"id": "Jt6paovLrLvcBHmawJKR-294",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-287"
},
{
"id": "Jt6paovLrLvcBHmawJKR-295",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-294",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-296",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-294",
"label": "RequesterEmail : nvarchar(50)"
},
{
"id": "Jt6paovLrLvcBHmawJKR-297",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-287"
},
{
"id": "Jt6paovLrLvcBHmawJKR-298",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-297",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-299",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-297",
"label": "ProposedStart : dateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-300",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-287"
},
{
"id": "Jt6paovLrLvcBHmawJKR-301",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-300",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-302",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-300",
"label": "ProposedEnd : dateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-303",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-287"
},
{
"id": "Jt6paovLrLvcBHmawJKR-304",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-303",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-305",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-303",
"label": "Reason : nvarchar(100)"
},
{
"id": "Jt6paovLrLvcBHmawJKR-306",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-287"
},
{
"id": "Jt6paovLrLvcBHmawJKR-307",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-306",
"label": "*"
},
{
"id": "Jt6paovLrLvcBHmawJKR-308",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-306",
"label": "Status : varchar(30)"
},
{
"id": "Jt6paovLrLvcBHmawJKR-309",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-287"
},
{
"id": "Jt6paovLrLvcBHmawJKR-310",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-309"
},
{
"id": "Jt6paovLrLvcBHmawJKR-311",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-309",
"label": "ReviewedBy : guid"
},
{
"id": "Jt6paovLrLvcBHmawJKR-312",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-287"
},
{
"id": "Jt6paovLrLvcBHmawJKR-313",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-312"
},
{
"id": "Jt6paovLrLvcBHmawJKR-314",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-312",
"label": "ReviewedAt : dateTime()"
},
{
"id": "Jt6paovLrLvcBHmawJKR-315",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-287"
},
{
"id": "Jt6paovLrLvcBHmawJKR-316",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-315"
},
{
"id": "Jt6paovLrLvcBHmawJKR-317",
"type": "node",
"parent": "Jt6paovLrLvcBHmawJKR-315",
"label": "ReviewNotes : nvarchar(100)"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-202",
"type": "edge",
"parent": "1",
"source": "Ni1uZt9kZrgzDhyM-i3v-41",
"target": "Ni1uZt9kZrgzDhyM-i3v-152"
},
{
"id": "Ni1uZt9kZrgzDhyM-i3v-204",
"type": "edge",
"parent": "1",
"source": "Ni1uZt9kZrgzDhyM-i3v-149",
"target": "Ni1uZt9kZrgzDhyM-i3v-193"
},
{
"id": "Jt6paovLrLvcBHmawJKR-384",
"type": "edge",
"parent": "1",
"source": "Jt6paovLrLvcBHmawJKR-97",
"target": "Ni1uZt9kZrgzDhyM-i3v-109"
},
{
"id": "Jt6paovLrLvcBHmawJKR-385",
"type": "edge",
"parent": "1",
"source": "9bh975Iajj_f-3G999F8-2",
"target": "Ni1uZt9kZrgzDhyM-i3v-142"
},
{
"id": "vu0sGOmyCkF3KWFEINvx-5",
"type": "edge",
"parent": "1",
"source": "Ni1uZt9kZrgzDhyM-i3v-106",
"target": "Ni1uZt9kZrgzDhyM-i3v-155"
},
{
"id": "nfNeyiAOdsavxUQbhxos-54",
"type": "edge",
"parent": "1",
"source": "LyIj3it893yShO42AiGY-2",
"target": "nfNeyiAOdsavxUQbhxos-51"
},
{
"id": "nfNeyiAOdsavxUQbhxos-57",
"type": "node",
"parent": "1",
"label": "QuestionBank"
},
{
"id": "nfNeyiAOdsavxUQbhxos-58",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-57"
},
{
"id": "nfNeyiAOdsavxUQbhxos-59",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-58",
"label": "PK"
},
{
"id": "nfNeyiAOdsavxUQbhxos-60",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-58",
"label": "Id GUID"
},
{
"id": "nfNeyiAOdsavxUQbhxos-61",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-57"
},
{
"id": "nfNeyiAOdsavxUQbhxos-62",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-61",
"label": "FK"
},
{
"id": "nfNeyiAOdsavxUQbhxos-63",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-61",
"label": "Department GUID"
},
{
"id": "nfNeyiAOdsavxUQbhxos-88",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-57"
},
{
"id": "nfNeyiAOdsavxUQbhxos-89",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-88",
"label": "FK"
},
{
"id": "nfNeyiAOdsavxUQbhxos-90",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-88",
"label": "StageType GUID"
},
{
"id": "nfNeyiAOdsavxUQbhxos-79",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-57"
},
{
"id": "nfNeyiAOdsavxUQbhxos-80",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-79",
"label": "*"
},
{
"id": "nfNeyiAOdsavxUQbhxos-81",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-79",
"label": "CreatedBy : GUID"
},
{
"id": "nfNeyiAOdsavxUQbhxos-92",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-57"
},
{
"id": "nfNeyiAOdsavxUQbhxos-93",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-92",
"label": "*"
},
{
"id": "nfNeyiAOdsavxUQbhxos-94",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-92",
"label": "CreatedAt : DateTime()"
},
{
"id": "nfNeyiAOdsavxUQbhxos-82",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-57"
},
{
"id": "nfNeyiAOdsavxUQbhxos-83",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-82"
},
{
"id": "nfNeyiAOdsavxUQbhxos-84",
"type": "node",
"parent": "nfNeyiAOdsavxUQbhxos-82",
"label": "UpdatedAt : DateTime()"
},
{
"id": "nfNeyiAOdsavxUQbhxos-91",
"type": "edge",
"parent": "1",
"source": "Jt6paovLrLvcBHmawJKR-139",
"target": "nfNeyiAOdsavxUQbhxos-88"
},
{
"id": "nfNeyiAOdsavxUQbhxos-95",
"type": "edge",
"parent": "1",
"source": "nfNeyiAOdsavxUQbhxos-58",
"target": "Jt6paovLrLvcBHmawJKR-7"
},
{
"id": "nfNeyiAOdsavxUQbhxos-96",
"type": "edge",
"parent": "1",
"source": "LyIj3it893yShO42AiGY-2",
"target": "nfNeyiAOdsavxUQbhxos-61"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-7",
"type": "node",
"parent": "1",
"label": "EmailLog"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-8",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-7"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-9",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-8",
"label": "PK"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-10",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-8",
"label": "Id GUID"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-14",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-7"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-15",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-14",
"label": "*"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-16",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-14",
"label": "SenderEmail : nvarchar(50)"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-20",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-7"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-21",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-20",
"label": "*"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-22",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-20",
"label": "ReceiptEmail : nvarchar(50)"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-17",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-7"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-18",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-17",
"label": "*"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-19",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-17",
"label": "Type : StageName"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-23",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-7"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-24",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-23",
"label": "*"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-25",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-23",
"label": "Subject : nvarchar(100)"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-26",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-7"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-27",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-26",
"label": "*"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-28",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-26",
"label": "Body : nvarchar(max)"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-29",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-7"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-30",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-29",
"label": "*"
},
{
"id": "E3xaFXgcV9SUiTC7jBoP-31",
"type": "node",
"parent": "E3xaFXgcV9SUiTC7jBoP-29",
"label": "SendAt : DateTime()"
}
]
}
]
}