#Drawing and compare shapes game for Murka


##В соответствии с техническим заданием (ТЗ) (файл [PRD](PRD.pdf)) была разработана игра MurkaDraw.

В папке [Bin](Bin) лежат архивы собранной игры под разные ОС.

##Игровой процесс:

В меню можно начать игру или войти в режим добавления новых фигур.
При старте нового уровня игра выдаёт одну из фигур и игрок должен её нарисовать зажав левую кнопку мышки (ЛКМ) и начав водить по игровой области, при отпускании ЛКМ будет проведено сравнение с фигурой задания. Если фигура игрока похожа на фигуру в задании то будет дано новое задание, иначе, старая фигура игрока поменяет цвет на серый и пропадёт при начале новой попытки нарисовать что-то.
На каждый новый уровень даётся меньше на 5 секунд времени чем на предыдущем, начальное время на фигуру 60 секунд, так что в лучшем случае можно нарисовать 12 фигур.

Для возможности переключения между игровыми экранами всегда есть кнопка меню при рисовании.

##Добавление новых фигур:

###1-й способ
В редакторе рисование фигур происходит таким-же образом как и в игре, но при отпускании ЛКМ игроку предлагается сохранить фигуру или выйти. При сохранении фигуры можно рисовать новую.

###2-й способ
Так-же добавлять фигуры можно в [коде](Assets/Scripts/MainSrc.cs), список Shapes содержит структуру фигур, которая в свою очередь просто массив вершин.

Достаточно добавлять строки такого вида:

    Shapes.Add(new Shape(new Vector2[] { new Vector2(-3, -2), new Vector2(3, -2), new Vector2(0, 2) }));


##Фигуры сравниваются следующим методом:

- первым делом получаем список вершин фигуры задания и рисунка игрока;
- так как в ТЗ сказано о разном масштабе, то предварительно рисунок игрока подгоняется размерами к фигуре задания (то что получилось при неправильном рисунке видно серыми линиями на экране игры);
- проводится поиск попадания вершин рисунка игрока на вершины фигуры задания, найденные вершины в рисунке игрока убираються, а соответствующие им в фигуре задания помечаются для возможности отсечь недорисованных вариантов (вместо квадрата нарисовать П, и другие случаи). Так как фигуры задания могут содержать много вершин, то достаточно чтоб совпало 90% вершин;
- оставшиеся вершины рисунка игрока проверяются на отдаление от отрезков из вершин фигуры задания, все вершины что близко к отрезкам удаляются из рисунка игрока;
- для определения совпадения рисунка игрока после всех манипуляций список вершин рисунка должен быть пустым, а совпадений с вершинами фигуры задания должно быть не меньше 90%;

##Дополнительно:

Параметры начального времени на задание, шаг изменения времени и допустимое отдаление при сравнении вершин рисунка от вершин фигуры задания или их отрезков можно менять в редакторе Юнити.


##Что и как можно улучшить:

- для более точного определения совпадений фигуры с рисунком необходимо поменять условие проверки совпадения вершин. Сравнивать не каждую из оставшихся вершин рисунка с текущей вершиной фигуры задания, а на оборот, брать каждую вершину рисунка и искать с какими вершинами фигуры задания она совпадает, а по окончанию уже удалять её из списка. Таким образом будут учтены для близ стоящих вершин фигуры задания общие вершины рисунка игрока и не нужно будет задавать 90%, а сразу проверять что все вершины были охвачены игроком;
- вынести класс фигур в отдельный файл и перенести все методы работы с фигурами в него отвязав от глобальных параметров, получим полноценный оправданный класс для хранения и работы с фигурами. Получим более лаконичный листинг кода;
