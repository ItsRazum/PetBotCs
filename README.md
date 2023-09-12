# PetBotCs
Open-source Bot with included mini-game and SQL-database Read/Write system.


Бот полностью настраиваемый, можно подключать его к своему боту и менять сообщения в коде на те, на которые вы пожелаете.
Таблицы групп в базе данных создаются автоматически, однако таблица дуэлей имеет свой требуемый формат:
название таблицы: duels
Она должна состоять из следующих столбцов:
p1id	p2id	p1pos	 p2pos 	p1hp	p2hp	p1name	p2name	rootgroup	isAllowed  p1IsReady	p2IsReady 	p1IsMoved 	p2IsMoved 	IsFriendly

Обязательно создайте эту таблицу! Иначе бот работать не будет!
Документация к боту: https://petbotcs.gitbook.io/petbotcs/
