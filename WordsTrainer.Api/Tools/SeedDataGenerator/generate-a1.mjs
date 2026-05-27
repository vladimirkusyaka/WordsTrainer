import { mkdir, writeFile } from "node:fs/promises";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import additionalRows from "./a1-additional.mjs";

const rows = [
  ["house_home", "noun", "das Haus", "дом", "house", "Здание, где живут люди.", "A building where people live.", "Ein Gebaeude, in dem Menschen wohnen.", "Ich wohne in einem kleinen Haus.", "Я живу в маленьком доме.", "I live in a small house."],
  ["apartment", "noun", "die Wohnung", "квартира", "apartment", "Жилое помещение в многоквартирном доме.", "A home that is part of a larger building.", "Eine Unterkunft in einem groesseren Wohngebaeude.", "Unsere Wohnung hat zwei Zimmer.", "В нашей квартире две комнаты.", "Our apartment has two rooms."],
  ["room", "noun", "das Zimmer", "комната", "room", "Отдельное помещение внутри дома или квартиры.", "A separate space inside a home or building.", "Ein einzelner Raum in einer Wohnung oder einem Haus.", "Mein Zimmer ist hell.", "Моя комната светлая.", "My room is bright."],
  ["door", "noun", "die Tuer", "дверь", "door", "Часть стены, которую открывают, чтобы войти или выйти.", "A movable barrier used to enter or leave a room.", "Eine bewegliche Oeffnung zum Ein- oder Ausgehen.", "Bitte schliess die Tuer.", "Пожалуйста, закрой дверь.", "Please close the door."],
  ["window", "noun", "das Fenster", "окно", "window", "Проём со стеклом, через который видны улица и свет.", "An opening with glass that lets light into a room.", "Eine verglaste Oeffnung, die Licht in einen Raum laesst.", "Das Fenster ist offen.", "Окно открыто.", "The window is open."],
  ["table", "noun", "der Tisch", "стол", "table", "Предмет мебели с ровной поверхностью для еды или работы.", "A piece of furniture with a flat top for eating or working.", "Ein Moebelstueck mit einer Flaeche zum Essen oder Arbeiten.", "Das Buch liegt auf dem Tisch.", "Книга лежит на столе.", "The book is on the table."],
  ["chair", "noun", "der Stuhl", "стул", "Предмет мебели, на котором сидит один человек.", "A seat for one person.", "Ein Sitzmoebel fuer eine Person.", "Der Stuhl steht neben dem Tisch.", "Стул стоит рядом со столом.", "The chair is beside the table."],
  ["bed", "noun", "das Bett", "кровать", "Мебель, на которой спят.", "Furniture used for sleeping.", "Ein Moebelstueck, in dem man schlaeft.", "Das Kind schlaeft im Bett.", "Ребёнок спит в кровати.", "The child is sleeping in bed."],
  ["kitchen", "noun", "die Kueche", "кухня", "Комната, где готовят еду.", "A room where food is prepared.", "Ein Raum, in dem Essen zubereitet wird.", "Ich koche in der Kueche.", "Я готовлю на кухне.", "I cook in the kitchen."],
  ["bathroom", "noun", "das Badezimmer", "ванная комната", "Комната с ванной или душем для мытья.", "A room with a bath or shower for washing.", "Ein Raum mit Badewanne oder Dusche zum Waschen.", "Das Badezimmer ist links.", "Ванная комната слева.", "The bathroom is on the left."],
  ["family", "noun", "die Familie", "семья", "Родители, дети и другие близкие родственники.", "Parents, children, and close relatives.", "Eltern, Kinder und nahe Verwandte.", "Meine Familie lebt in Berlin.", "Моя семья живёт в Берлине.", "My family lives in Berlin."],
  ["mother", "noun", "die Mutter", "мать", "Женщина по отношению к своему ребёнку.", "A female parent.", "Eine Frau in Beziehung zu ihrem Kind.", "Meine Mutter arbeitet heute.", "Моя мама сегодня работает.", "My mother is working today."],
  ["father", "noun", "der Vater", "отец", "Мужчина по отношению к своему ребёнку.", "A male parent.", "Ein Mann in Beziehung zu seinem Kind.", "Mein Vater liest die Zeitung.", "Мой отец читает газету.", "My father is reading the newspaper."],
  ["brother", "noun", "der Bruder", "брат", "Мальчик или мужчина с теми же родителями.", "A boy or man who shares your parents.", "Ein Junge oder Mann mit denselben Eltern.", "Mein Bruder spielt Fussball.", "Мой брат играет в футбол.", "My brother plays football."],
  ["sister", "noun", "die Schwester", "сестра", "Девочка или женщина с теми же родителями.", "A girl or woman who shares your parents.", "Ein Maedchen oder eine Frau mit denselben Eltern.", "Meine Schwester spricht Deutsch.", "Моя сестра говорит по-немецки.", "My sister speaks German."],
  ["child", "noun", "das Kind", "ребёнок", "Маленький человек; сын или дочь.", "A young person; a son or daughter.", "Ein junger Mensch; ein Sohn oder eine Tochter.", "Das Kind malt ein Bild.", "Ребёнок рисует картину.", "The child is drawing a picture."],
  ["friend", "noun", "der Freund", "друг", "Человек, которого хорошо знаешь и которому доверяешь.", "A person you know well and like.", "Eine Person, die man gut kennt und mag.", "Mein Freund kommt am Abend.", "Мой друг придёт вечером.", "My friend is coming in the evening."],
  ["woman", "noun", "die Frau", "женщина", "Взрослый человек женского пола; также жена в контексте.", "An adult female person; also wife in context.", "Eine erwachsene weibliche Person; im Kontext auch Ehefrau.", "Die Frau wartet an der Haltestelle.", "Женщина ждёт на остановке.", "The woman is waiting at the stop."],
  ["man", "noun", "der Mann", "мужчина", "Взрослый человек мужского пола; также муж в контексте.", "An adult male person; also husband in context.", "Eine erwachsene maennliche Person; im Kontext auch Ehemann.", "Der Mann traegt eine Jacke.", "Мужчина носит куртку.", "The man is wearing a jacket."],
  ["name", "noun", "der Name", "имя", "Слово, которым называют человека или предмет.", "A word used to identify a person or thing.", "Ein Wort, mit dem eine Person oder Sache bezeichnet wird.", "Mein Name ist Anna.", "Меня зовут Анна.", "My name is Anna."],
  ["water", "noun", "das Wasser", "вода", "Прозрачная жидкость, которую пьют.", "The clear liquid people drink.", "Eine klare Fluessigkeit, die Menschen trinken.", "Ich trinke ein Glas Wasser.", "Я пью стакан воды.", "I drink a glass of water."],
  ["bread", "noun", "das Brot", "хлеб", "Пища из муки, обычно испечённая.", "Food made from flour and usually baked.", "Ein gebackenes Lebensmittel aus Mehl.", "Zum Fruehstueck esse ich Brot.", "На завтрак я ем хлеб.", "I eat bread for breakfast."],
  ["milk", "noun", "die Milch", "молоко", "Белый напиток животного или растительного происхождения.", "A white drink from animals or plants.", "Ein weisses Getraenk tierischen oder pflanzlichen Ursprungs.", "Sie kauft Milch im Supermarkt.", "Она покупает молоко в супермаркете.", "She buys milk at the supermarket."],
  ["coffee", "noun", "der Kaffee", "кофе", "Горячий напиток из кофейных зёрен.", "A hot drink made from coffee beans.", "Ein heisses Getraenk aus Kaffeebohnen.", "Morgens trinke ich Kaffee.", "Утром я пью кофе.", "I drink coffee in the morning."],
  ["tea", "noun", "der Tee", "чай", "Горячий напиток, который заваривают в воде.", "A hot drink made by infusing leaves in water.", "Ein heisses Getraenk, das in Wasser aufgegossen wird.", "Moechtest du Tee?", "Ты хочешь чай?", "Would you like tea?"],
  ["apple", "noun", "der Apfel", "яблоко", "Круглый фрукт, часто красный или зелёный.", "A round fruit, often red or green.", "Eine runde Frucht, oft rot oder gruen.", "Ich esse einen Apfel.", "Я ем яблоко.", "I am eating an apple."],
  ["banana", "noun", "die Banane", "банан", "Длинный жёлтый фрукт с мягкой мякотью.", "A long yellow fruit with soft flesh.", "Eine lange gelbe Frucht mit weichem Inneren.", "Die Banane ist reif.", "Банан спелый.", "The banana is ripe."],
  ["breakfast", "noun", "das Fruehstueck", "завтрак", "Первая еда дня, обычно утром.", "The first meal of the day, usually eaten in the morning.", "Die erste Mahlzeit des Tages, meist am Morgen.", "Das Fruehstueck ist um acht Uhr.", "Завтрак в восемь часов.", "Breakfast is at eight o'clock."],
  ["food", "noun", "das Essen", "еда", "То, что люди едят; также приём пищи.", "Things people eat; also a meal.", "Was Menschen essen; auch eine Mahlzeit.", "Das Essen ist fertig.", "Еда готова.", "The food is ready."],
  ["restaurant", "noun", "das Restaurant", "ресторан", "Место, где заказывают и едят приготовленную еду.", "A place where prepared meals are ordered and eaten.", "Ein Ort, an dem man zubereitete Speisen bestellt und isst.", "Wir essen heute im Restaurant.", "Сегодня мы едим в ресторане.", "We are eating at a restaurant today."],
  ["school", "noun", "die Schule", "школа", "Место, где дети или взрослые учатся.", "A place where children or adults learn.", "Ein Ort, an dem Kinder oder Erwachsene lernen.", "Die Schule beginnt um acht.", "Школа начинается в восемь.", "School starts at eight."],
  ["teacher", "noun", "der Lehrer", "учитель", "Человек, который обучает других.", "A person who teaches others.", "Eine Person, die andere unterrichtet.", "Der Lehrer erklaert die Aufgabe.", "Учитель объясняет задание.", "The teacher explains the task."],
  ["book", "noun", "das Buch", "книга", "Печатное или электронное произведение со страницами и текстом.", "A printed or digital work with pages and text.", "Ein gedrucktes oder digitales Werk mit Seiten und Text.", "Ich lese ein deutsches Buch.", "Я читаю немецкую книгу.", "I am reading a German book."],
  ["pen", "noun", "der Stift", "ручка", "Предмет, которым пишут или рисуют.", "An object used for writing or drawing.", "Ein Gegenstand zum Schreiben oder Zeichnen.", "Hast du einen Stift?", "У тебя есть ручка?", "Do you have a pen?"],
  ["word", "noun", "das Wort", "слово", "Единица языка со значением.", "A unit of language that has meaning.", "Eine sprachliche Einheit mit einer Bedeutung.", "Dieses Wort ist neu fuer mich.", "Это слово для меня новое.", "This word is new to me."],
  ["language", "noun", "die Sprache", "язык", "Система слов и правил, на которой говорят люди.", "A system of words and rules used for communication.", "Ein System von Woertern und Regeln zur Kommunikation.", "Deutsch ist eine interessante Sprache.", "Немецкий - интересный язык.", "German is an interesting language."],
  ["work_job", "noun", "die Arbeit", "работа", "Деятельность или место, связанные с профессией.", "Activity or a place connected with a job.", "Eine Taetigkeit oder ein Ort im Zusammenhang mit einem Beruf.", "Ich gehe zur Arbeit.", "Я иду на работу.", "I am going to work."],
  ["office", "noun", "das Buero", "офис", "Помещение, где люди выполняют рабочие задачи.", "A room or place where people do office work.", "Ein Raum, in dem Menschen Bueroarbeit erledigen.", "Sie arbeitet im Buero.", "Она работает в офисе.", "She works in an office."],
  ["shop", "noun", "das Geschaeft", "магазин", "Место, где покупают товары.", "A place where goods are bought.", "Ein Ort, an dem man Waren kauft.", "Das Geschaeft ist heute geschlossen.", "Магазин сегодня закрыт.", "The shop is closed today."],
  ["money", "noun", "das Geld", "деньги", "То, чем платят за товары и услуги.", "What people use to pay for goods and services.", "Was man zum Bezahlen von Waren und Dienstleistungen benutzt.", "Ich habe kein Bargeld dabei.", "У меня нет с собой наличных денег.", "I do not have cash with me."],
  ["city", "noun", "die Stadt", "город", "Большое населённое место с улицами и зданиями.", "A large populated place with streets and buildings.", "Ein groesserer bewohnter Ort mit Strassen und Gebaeuden.", "Die Stadt ist sehr alt.", "Город очень старый.", "The city is very old."],
  ["street", "noun", "die Strasse", "улица", "Дорога в городе или населённом пункте.", "A road in a city or town.", "Eine Strasse in einer Stadt oder einem Ort.", "Unsere Strasse ist ruhig.", "Наша улица тихая.", "Our street is quiet."],
  ["station", "noun", "der Bahnhof", "вокзал", "Место, где останавливаются поезда.", "A place where trains arrive and depart.", "Ein Ort, an dem Zuege ankommen und abfahren.", "Der Zug wartet am Bahnhof.", "Поезд ждёт на вокзале.", "The train is waiting at the station."],
  ["train", "noun", "der Zug", "поезд", "Транспорт, который движется по рельсам.", "A vehicle that travels on rails.", "Ein Verkehrsmittel, das auf Schienen faehrt.", "Der Zug kommt um neun Uhr.", "Поезд прибывает в девять часов.", "The train arrives at nine o'clock."],
  ["bus", "noun", "der Bus", "автобус", "Большой дорожный транспорт для пассажиров.", "A large road vehicle for passengers.", "Ein grosses Strassenfahrzeug fuer Fahrgaeste.", "Ich fahre mit dem Bus.", "Я еду на автобусе.", "I travel by bus."],
  ["car", "noun", "das Auto", "автомобиль", "Легковое транспортное средство на дороге.", "A passenger vehicle used on roads.", "Ein Fahrzeug fuer Personen auf der Strasse.", "Das Auto steht vor dem Haus.", "Машина стоит перед домом.", "The car is in front of the house."],
  ["ticket", "noun", "die Fahrkarte", "билет", "Документ или запись, разрешающие поездку.", "A document or record that permits a journey.", "Ein Dokument oder Nachweis fuer eine Fahrt.", "Ich brauche eine Fahrkarte nach Hamburg.", "Мне нужен билет до Гамбурга.", "I need a ticket to Hamburg."],
  ["day", "noun", "der Tag", "день", "Период от утра до ночи или сутки в календаре.", "The period from morning to night or a calendar day.", "Die Zeit von Morgen bis Nacht oder ein Kalendertag.", "Heute ist ein schoener Tag.", "Сегодня прекрасный день.", "Today is a beautiful day."],
  ["week", "noun", "die Woche", "неделя", "Период из семи дней.", "A period of seven days.", "Ein Zeitraum von sieben Tagen.", "Ich arbeite fuenf Tage pro Woche.", "Я работаю пять дней в неделю.", "I work five days a week."],
  ["morning", "noun", "der Morgen", "утро", "Начальная часть дня до полудня.", "The early part of the day before noon.", "Der fruehe Teil des Tages vor Mittag.", "Am Morgen trinke ich Tee.", "Утром я пью чай.", "I drink tea in the morning."],
  ["evening", "noun", "der Abend", "вечер", "Часть дня после работы и перед ночью.", "The part of the day after afternoon and before night.", "Der Teil des Tages nach dem Nachmittag und vor der Nacht.", "Am Abend sehe ich einen Film.", "Вечером я смотрю фильм.", "I watch a film in the evening."],
  ["time", "noun", "die Zeit", "время", "То, что измеряют часами; свободный или назначенный период.", "What clocks measure; an available or scheduled period.", "Was Uhren messen; ein verfuegbarer oder geplanter Zeitraum.", "Ich habe heute wenig Zeit.", "У меня сегодня мало времени.", "I have little time today."],
  ["weather", "noun", "das Wetter", "погода", "Состояние воздуха и неба в определённое время.", "The condition of the air and sky at a particular time.", "Der Zustand von Luft und Himmel zu einer bestimmten Zeit.", "Das Wetter ist heute gut.", "Сегодня хорошая погода.", "The weather is good today."],
  ["sun", "noun", "die Sonne", "солнце", "Звезда, которая даёт Земле свет и тепло.", "The star that gives Earth light and warmth.", "Der Stern, der der Erde Licht und Waerme gibt.", "Die Sonne scheint.", "Солнце светит.", "The sun is shining."],
  ["rain", "noun", "der Regen", "дождь", "Вода, падающая каплями из облаков.", "Water that falls in drops from clouds.", "Wasser, das in Tropfen aus Wolken faellt.", "Der Regen beginnt am Nachmittag.", "Дождь начнётся днём.", "The rain starts in the afternoon."],
  ["dog", "noun", "der Hund", "собака", "Домашнее животное, которое часто живёт рядом с человеком.", "A domestic animal often kept as a companion.", "Ein Haustier, das oft mit Menschen lebt.", "Der Hund laeuft im Park.", "Собака бегает в парке.", "The dog runs in the park."],
  ["cat", "noun", "die Katze", "кошка", "Небольшое домашнее животное с мягкой шерстью.", "A small domestic animal with soft fur.", "Ein kleines Haustier mit weichem Fell.", "Die Katze schlaeft auf dem Sofa.", "Кошка спит на диване.", "The cat is sleeping on the sofa."],
  ["eat", "verb", "essen", "есть", "Принимать пищу.", "To take food into the body.", "Nahrung zu sich nehmen.", "Wir essen zusammen zu Abend.", "Мы вместе ужинаем.", "We eat dinner together."],
  ["drink", "verb", "trinken", "пить", "Принимать жидкость.", "To take liquid into the body.", "Fluessigkeit zu sich nehmen.", "Du solltest mehr Wasser trinken.", "Тебе стоит пить больше воды.", "You should drink more water."],
  ["go_walk", "verb", "gehen", "идти", "Передвигаться пешком или направляться куда-либо.", "To move on foot or travel to a place.", "Sich zu Fuss bewegen oder zu einem Ort begeben.", "Ich gehe heute zur Schule.", "Сегодня я иду в школу.", "I am going to school today."],
  ["come", "verb", "kommen", "приходить", "Двигаться к говорящему или в указанное место.", "To move toward the speaker or a specified place.", "Sich zum Sprecher oder zu einem genannten Ort bewegen.", "Wann kommst du nach Hause?", "Когда ты придёшь домой?", "When are you coming home?"],
  ["live_reside", "verb", "wohnen", "жить, проживать", "Иметь дом или квартиру в определённом месте.", "To have one's home in a particular place.", "An einem bestimmten Ort sein Zuhause haben.", "Wir wohnen in Koeln.", "Мы живём в Кёльне.", "We live in Cologne."],
  ["work_verb", "verb", "arbeiten", "работать", "Выполнять профессиональную или практическую деятельность.", "To do a job or practical activity.", "Eine berufliche oder praktische Taetigkeit ausueben.", "Er arbeitet von Montag bis Freitag.", "Он работает с понедельника по пятницу.", "He works from Monday to Friday."],
  ["learn", "verb", "lernen", "учить, изучать", "Получать новые знания или навыки.", "To gain new knowledge or skills.", "Neue Kenntnisse oder Faehigkeiten erwerben.", "Ich lerne jeden Tag Deutsch.", "Я каждый день учу немецкий.", "I learn German every day."],
  ["read", "verb", "lesen", "читать", "Понимать написанный текст.", "To understand written text.", "Geschriebenen Text verstehen.", "Sie liest ein Buch im Zug.", "Она читает книгу в поезде.", "She reads a book on the train."],
  ["write", "verb", "schreiben", "писать", "Создавать слова и предложения буквами.", "To create words and sentences with letters.", "Woerter und Saetze mit Buchstaben bilden.", "Ich schreibe eine Nachricht.", "Я пишу сообщение.", "I am writing a message."],
  ["speak", "verb", "sprechen", "говорить", "Выражать мысли словами вслух.", "To express thoughts aloud in words.", "Gedanken mit gesprochenen Woertern ausdruecken.", "Sprichst du Englisch?", "Ты говоришь по-английски?", "Do you speak English?"],
  ["listen", "verb", "hoeren", "слушать, слышать", "Воспринимать звук ушами или внимательно слушать.", "To perceive sound or pay attention to it.", "Schall wahrnehmen oder aufmerksam zuhoeren.", "Ich hoere gern Musik.", "Я люблю слушать музыку.", "I like listening to music."],
  ["see", "verb", "sehen", "видеть", "Воспринимать глазами.", "To perceive with the eyes.", "Mit den Augen wahrnehmen.", "Ich sehe den Bus.", "Я вижу автобус.", "I see the bus."],
  ["buy", "verb", "kaufen", "покупать", "Получать вещь, заплатив за неё.", "To get something by paying for it.", "Etwas gegen Bezahlung erhalten.", "Wir kaufen Obst auf dem Markt.", "Мы покупаем фрукты на рынке.", "We buy fruit at the market."],
  ["pay", "verb", "bezahlen", "платить", "Отдавать деньги за товар или услугу.", "To give money for goods or services.", "Geld fuer Waren oder Dienstleistungen geben.", "Kann ich mit Karte bezahlen?", "Я могу заплатить картой?", "Can I pay by card?"],
  ["sleep", "verb", "schlafen", "спать", "Отдыхать в состоянии сна.", "To rest in a state of sleep.", "Im Zustand des Schlafes ruhen.", "Das Baby schlaeft schon.", "Малыш уже спит.", "The baby is already sleeping."],
  ["cook", "verb", "kochen", "готовить", "Готовить пищу, часто с нагреванием.", "To prepare food, often by heating it.", "Essen zubereiten, oft durch Erhitzen.", "Heute koche ich eine Suppe.", "Сегодня я готовлю суп.", "Today I am cooking soup."],
  ["help", "verb", "helfen", "помогать", "Делать что-либо легче для другого человека.", "To make something easier for another person.", "Etwas fuer eine andere Person leichter machen.", "Kannst du mir helfen?", "Ты можешь мне помочь?", "Can you help me?"],
  ["open", "verb", "oeffnen", "открывать", "Делать доступным вход или содержимое.", "To make an entrance or contents accessible.", "Einen Zugang oder Inhalt zugaenglich machen.", "Bitte oeffnen Sie das Fenster.", "Пожалуйста, откройте окно.", "Please open the window."],
  ["close", "verb", "schliessen", "закрывать", "Делать вход или предмет закрытым.", "To make an entrance or object closed.", "Einen Zugang oder Gegenstand zumachen.", "Ich schliesse die Tuer.", "Я закрываю дверь.", "I close the door."],
  ["have", "verb", "haben", "иметь", "Обладать чем-либо или располагать чем-либо.", "To possess or have something available.", "Etwas besitzen oder zur Verfuegung haben.", "Ich habe eine Frage.", "У меня есть вопрос.", "I have a question."],
  ["be", "verb", "sein", "быть", "Находиться в состоянии или являться кем-либо.", "To exist or be in a particular state.", "Existieren oder sich in einem Zustand befinden.", "Ich bin heute zu Hause.", "Я сегодня дома.", "I am at home today."],
  ["good", "adjective", "gut", "хороший", "Имеющий положительное качество или подходящий.", "Having a positive quality or being suitable.", "Von positiver Qualitaet oder passend.", "Das ist eine gute Idee.", "Это хорошая идея.", "That is a good idea."],
  ["bad", "adjective", "schlecht", "плохой", "Низкого качества или нежелательный.", "Of low quality or undesirable.", "Von geringer Qualitaet oder unerwuenscht.", "Das Wetter ist schlecht.", "Погода плохая.", "The weather is bad."],
  ["big", "adjective", "gross", "большой", "Значительный по размеру.", "Large in size.", "Von bedeutender Groesse.", "Berlin ist eine grosse Stadt.", "Берлин - большой город.", "Berlin is a big city."],
  ["small", "adjective", "klein", "маленький", "Небольшой по размеру.", "Not large in size.", "Von geringer Groesse.", "Wir haben eine kleine Kueche.", "У нас маленькая кухня.", "We have a small kitchen."],
  ["new", "adjective", "neu", "новый", "Недавно созданный, купленный или ещё не использованный.", "Recently made, bought, or not used before.", "Kuerzlich gemacht, gekauft oder noch nicht benutzt.", "Ich habe ein neues Fahrrad.", "У меня новый велосипед.", "I have a new bicycle."],
  ["old", "adjective", "alt", "старый", "Существующий долгое время или немолодой.", "Existing for a long time or not young.", "Seit langer Zeit vorhanden oder nicht jung.", "Das Haus ist sehr alt.", "Дом очень старый.", "The house is very old."],
  ["beautiful", "adjective", "schoen", "красивый", "Приятный для взгляда или впечатления.", "Pleasant to look at or experience.", "Angenehm anzusehen oder zu erleben.", "Der Park ist im Sommer schoen.", "Летом парк красивый.", "The park is beautiful in summer."],
  ["fast", "adjective", "schnell", "быстрый", "Движущийся или происходящий за короткое время.", "Moving or happening in a short time.", "Sich in kurzer Zeit bewegend oder ereignend.", "Der Zug ist schnell.", "Поезд быстрый.", "The train is fast."],
  ["slow", "adjective", "langsam", "медленный", "Движущийся или происходящий не быстро.", "Moving or happening without speed.", "Sich nicht schnell bewegend oder ereignend.", "Der Bus ist heute langsam.", "Автобус сегодня медленный.", "The bus is slow today."],
  ["hot", "adjective", "heiss", "горячий", "Имеющий высокую температуру.", "Having a high temperature.", "Von hoher Temperatur.", "Der Kaffee ist noch heiss.", "Кофе ещё горячий.", "The coffee is still hot."],
  ["cold", "adjective", "kalt", "холодный", "Имеющий низкую температуру.", "Having a low temperature.", "Von niedriger Temperatur.", "Das Wasser ist kalt.", "Вода холодная.", "The water is cold."],
  ["happy", "adjective", "gluecklich", "счастливый", "Испытывающий радость или удовлетворение.", "Feeling joy or satisfaction.", "Freude oder Zufriedenheit empfindend.", "Sie ist heute sehr gluecklich.", "Она сегодня очень счастлива.", "She is very happy today."],
  ["tired", "adjective", "muede", "уставший", "Нуждающийся в отдыхе или сне.", "Needing rest or sleep.", "Ruhe oder Schlaf benoetigend.", "Nach der Arbeit bin ich muede.", "После работы я устал.", "I am tired after work."],
  ["yes", "adverb", "ja", "да", "Слово согласия или подтверждения.", "A word used to agree or confirm.", "Ein Wort fuer Zustimmung oder Bestaetigung.", "Ja, ich komme morgen.", "Да, я приду завтра.", "Yes, I will come tomorrow."],
  ["no", "adverb", "nein", "нет", "Слово отказа или отрицания.", "A word used to refuse or deny.", "Ein Wort fuer Ablehnung oder Verneinung.", "Nein, danke.", "Нет, спасибо.", "No, thank you."],
  ["please", "expression", "bitte", "пожалуйста", "Вежливое слово при просьбе или ответе на благодарность.", "A polite word used for requests or in response to thanks.", "Ein hoefliches Wort bei Bitten oder als Antwort auf Dank.", "Ein Wasser, bitte.", "Воду, пожалуйста.", "A water, please."],
  ["thanks", "expression", "danke", "спасибо", "Вежливое выражение благодарности.", "A polite expression of gratitude.", "Ein hoeflicher Ausdruck fuer Dankbarkeit.", "Danke fuer deine Hilfe.", "Спасибо за твою помощь.", "Thank you for your help."],
  ["hello", "expression", "hallo", "привет", "Приветствие при встрече.", "A greeting used when meeting someone.", "Eine Begruessung beim Treffen.", "Hallo, wie geht es dir?", "Привет, как ты?", "Hello, how are you?"],
  ["goodbye", "expression", "auf Wiedersehen", "до свидания", "Вежливое прощание.", "A polite expression used when leaving.", "Ein hoeflicher Abschiedsgruss.", "Auf Wiedersehen, bis morgen.", "До свидания, до завтра.", "Goodbye, see you tomorrow."],
  ["today", "adverb", "heute", "сегодня", "В текущий день.", "On the present day.", "Am gegenwaertigen Tag.", "Heute lerne ich zehn Woerter.", "Сегодня я учу десять слов.", "Today I learn ten words."],
  ["tomorrow", "adverb", "morgen", "завтра", "В день после сегодняшнего.", "On the day after today.", "Am Tag nach heute.", "Morgen besuche ich meine Familie.", "Завтра я навещу семью.", "Tomorrow I visit my family."],
  ["yesterday", "adverb", "gestern", "вчера", "В день перед сегодняшним.", "On the day before today.", "Am Tag vor heute.", "Gestern war ich im Kino.", "Вчера я был в кино.", "Yesterday I was at the cinema."],
  ["here", "adverb", "hier", "здесь", "В этом месте.", "In this place.", "An diesem Ort.", "Ich warte hier.", "Я жду здесь.", "I am waiting here."],
  ["there", "adverb", "dort", "там", "В другом указанном месте.", "In another indicated place.", "An einem anderen genannten Ort.", "Der Bahnhof ist dort.", "Вокзал там.", "The station is there."],
  ["left_direction", "adverb", "links", "налево, слева", "В направлении левой стороны или на левой стороне.", "Toward or on the left side.", "In Richtung der linken Seite oder auf der linken Seite.", "Die Apotheke ist links.", "Аптека слева.", "The pharmacy is on the left."],
  ["right_direction", "adverb", "rechts", "направо, справа", "В направлении правой стороны или на правой стороне.", "Toward or on the right side.", "In Richtung der rechten Seite oder auf der rechten Seite.", "Bitte gehen Sie rechts.", "Пожалуйста, идите направо.", "Please go to the right."],
  ["where", "adverb", "wo", "где", "Вопросительное слово о месте.", "A question word asking about a place.", "Ein Fragewort fuer einen Ort.", "Wo ist die Toilette?", "Где туалет?", "Where is the toilet?"],
  ["what", "pronoun", "was", "что", "Вопросительное слово о предмете или действии.", "A question word asking about a thing or action.", "Ein Fragewort fuer eine Sache oder Handlung.", "Was machst du heute?", "Что ты делаешь сегодня?", "What are you doing today?"],
  ["who", "pronoun", "wer", "кто", "Вопросительное слово о человеке.", "A question word asking about a person.", "Ein Fragewort fuer eine Person.", "Wer ist das?", "Кто это?", "Who is that?"],
  ["how", "adverb", "wie", "как", "Вопросительное слово о способе или состоянии.", "A question word asking about manner or condition.", "Ein Fragewort fuer Art und Zustand.", "Wie heisst du?", "Как тебя зовут?", "What is your name?"],
  ["when", "adverb", "wann", "когда", "Вопросительное слово о времени.", "A question word asking about time.", "Ein Fragewort fuer einen Zeitpunkt.", "Wann faehrt der Zug?", "Когда отправляется поезд?", "When does the train leave?"],
  ["one", "number", "eins", "один", "Число 1.", "The number 1.", "Die Zahl 1.", "Ich nehme ein Brot.", "Я возьму один хлеб.", "I will take one loaf of bread."],
  ["two", "number", "zwei", "два", "Число 2.", "The number 2.", "Die Zahl 2.", "Wir brauchen zwei Fahrkarten.", "Нам нужны два билета.", "We need two tickets."],
  ["three", "number", "drei", "три", "Число 3.", "The number 3.", "Die Zahl 3.", "Das Zimmer hat drei Fenster.", "В комнате три окна.", "The room has three windows."]
];

rows.push(...additionalRows);

const germanCorrections = [
  ["Gebaeude", "Gebäude"], ["groesseren", "größeren"], ["groesserer", "größerer"],
  ["Wohngebaeude", "Wohngebäude"], ["Tuer", "Tür"], ["Oeffnung", "Öffnung"],
  ["oeffnen", "öffnen"], ["laesst", "lässt"], ["Moebelstueck", "Möbelstück"],
  ["Sitzmoebel", "Sitzmöbel"], ["Flaeche", "Fläche"], ["fuer", "für"], ["schlaeft", "schläft"],
  ["Kueche", "Küche"], ["Fussball", "Fußball"], ["Maedchen", "Mädchen"],
  ["maennliche", "männliche"], ["traegt", "trägt"], ["Fluessigkeit", "Flüssigkeit"],
  ["Fruehstueck", "Frühstück"], ["weisses", "weißes"], ["Getraenk", "Getränk"],
  ["heisses", "heißes"], ["heiss", "heiß"], ["Moechtest", "Möchtest"],
  ["gruen", "grün"], ["erklaert", "erklärt"], ["Woertern", "Wörtern"],
  ["Woerter", "Wörter"], ["Taetigkeit", "Tätigkeit"], ["Buero", "Büro"],
  ["Geschaeft", "Geschäft"], ["Strassen", "Straßen"], ["Strasse", "Straße"],
  ["Gebaeuden", "Gebäuden"], ["Zuege", "Züge"], ["faehrt", "fährt"],
  ["grosses", "großes"], ["grosse", "große"], ["gross", "groß"],
  ["Fahrgaeste", "Fahrgäste"], ["schoener", "schöner"], ["schoen", "schön"],
  ["fuenf", "fünf"], ["fruehe", "frühe"], ["verfuegbarer", "verfügbarer"],
  ["Waerme", "Wärme"], ["faellt", "fällt"], ["laeuft", "läuft"],
  ["Koeln", "Köln"], ["ausueben", "ausüben"], ["Faehigkeiten", "Fähigkeiten"],
  ["Saetze", "Sätze"], ["ausdruecken", "ausdrücken"], ["hoeren", "hören"],
  ["hoere", "höre"], ["zuhoeren", "zuhören"], ["zugaenglich", "zugänglich"],
  ["schliesse", "schließe"], ["schliess", "schließ"], ["Verfuegung", "Verfügung"], ["Qualitaet", "Qualität"],
  ["unerwuenscht", "unerwünscht"], ["Groesse", "Größe"], ["gluecklich", "glücklich"],
  ["muede", "müde"], ["Kuerzlich", "Kürzlich"], ["benoetigend", "benötigend"],
  ["Bestaetigung", "Bestätigung"], ["hoeflich", "höflich"],
  ["Begruessung", "Begrüßung"], ["Abschiedsgruss", "Abschiedsgruß"],
  ["gegenwaertigen", "gegenwärtigen"], ["heisst", "heißt"]
];

function german(text) {
  return germanCorrections.reduce(
    (result, [source, replacement]) => result.replaceAll(source, replacement),
    text
  );
}

const normalizedRows = rows.map((row) => {
  if (row.length === 10) {
    const inferredEnglishWord = row[0].split("_")[0];
    return [...row.slice(0, 4), inferredEnglishWord, ...row.slice(4)];
  }

  if (row.length !== 11) {
    throw new Error(`${row[0]}: expected 11 fields, received ${row.length}`);
  }

  return row;
});

const concepts = normalizedRows.map(([
  key,
  partOfSpeech,
  de,
  ru,
  en,
  ruMeaning,
  enMeaning,
  deMeaning,
  deExample,
  ruExample,
  enExample
]) => {
  const targetWord = german(de);
  const targetExample = german(deExample);

  return {
    key,
    level: "A1",
    partOfSpeech,
    description: enMeaning,
    words: [
      { languageCode: "de", text: targetWord },
      { languageCode: "ru", text: ru },
      { languageCode: "en", text: en }
    ],
    explanations: [
      { languageCode: "ru", text: `${ruMeaning} Пример: «${targetExample}» - «${ruExample}»` },
      { languageCode: "en", text: `${enMeaning} Example: "${targetExample}" - "${enExample}"` },
      { languageCode: "de", text: `${german(deMeaning)} Beispiel: "${targetExample}"` }
    ]
  };
});

const keys = new Set();
for (const concept of concepts) {
  if (keys.has(concept.key)) {
    throw new Error(`Duplicate key: ${concept.key}`);
  }
  keys.add(concept.key);
  for (const languageCode of ["de", "ru", "en"]) {
    if (!concept.words.some((word) => word.languageCode === languageCode && word.text.trim())) {
      throw new Error(`${concept.key}: missing ${languageCode} word`);
    }
    if (!concept.explanations.some((item) => item.languageCode === languageCode && item.text.trim())) {
      throw new Error(`${concept.key}: missing ${languageCode} explanation`);
    }
  }
}

const outDir = join(dirname(fileURLToPath(import.meta.url)), "SeedData");
await mkdir(outDir, { recursive: true });
await writeFile(join(outDir, "a1.json"), `${JSON.stringify(concepts, null, 2)}\n`, "utf8");

console.log(`Generated ${concepts.length} A1 concepts in ${join(outDir, "a1.json")}`);
