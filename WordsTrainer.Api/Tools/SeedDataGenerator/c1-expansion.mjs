const n = (row) => {
  const [key, de, ru, en, ruMeaning, enMeaning, deMeaning] = row;
  return [key, "noun", de, ru, en, ruMeaning, enMeaning, deMeaning,
    `Im Bericht spielt ${de} eine zentrale Rolle.`,
    `В отчёте подробно рассматривается тема «${ru}».`,
    `In the report, ${en} plays a central role.`];
};

const v = (row) => {
  const [key, de, ru, en, ruMeaning, enMeaning, deMeaning] = row;
  return [key, "verb", de, ru, en, ruMeaning, enMeaning, deMeaning,
    `Die Verantwortlichen müssen sorgfältig ${de}.`,
    `Ответственные лица должны тщательно ${ru}.`,
    `Those responsible must carefully ${en}.`];
};

const a = (row) => {
  const [key, de, ru, en, ruMeaning, enMeaning, deMeaning] = row;
  return [key, "adjective", de, ru, en, ruMeaning, enMeaning, deMeaning,
    `Die Analyse beschreibt die Entwicklung als ${de}.`,
    `Анализ характеризует развитие как «${ru}».`,
    `The analysis describes the development as ${en}.`];
};

export default [
  // Society, ethics, law and human development
  n(["human_dignity", "die Menschenwürde", "человеческое достоинство", "human dignity", "Неотъемлемая ценность каждого человека, требующая уважения.", "The inherent value of every person requiring respect.", "Der jedem Menschen innewohnende Wert, der Achtung verlangt."]),
  n(["civil_liberty", "die bürgerliche Freiheit", "гражданская свобода", "civil liberty", "Защищённая возможность гражданина действовать без необоснованного вмешательства.", "A protected ability of a citizen to act without unjustified interference.", "Eine geschützte Möglichkeit des Bürgers, ohne unbegründeten Eingriff zu handeln."]),
  n(["rule_of_law", "die Rechtsstaatlichkeit", "верховенство права", "rule of law", "Принцип, по которому власть подчиняется закону.", "The principle that public power is subject to law.", "Das Prinzip, nach dem staatliche Macht dem Recht unterliegt."]),
  n(["proportionality", "die Verhältnismäßigkeit", "соразмерность", "proportionality", "Требование не применять меры сильнее необходимого.", "The requirement not to use measures stronger than necessary.", "Die Anforderung, keine stärkeren Maßnahmen als nötig einzusetzen."]),
  n(["discretion", "der Ermessensspielraum", "свобода усмотрения", "discretion", "Возможность выбрать решение в пределах правил.", "The freedom to choose a decision within established rules.", "Die Möglichkeit, innerhalb festgelegter Regeln eine Entscheidung zu wählen."]),
  n(["infringement", "die Rechtsverletzung", "нарушение права", "infringement", "Действие, незаконно затрагивающее чужое право.", "An action unlawfully affecting another person's right.", "Eine Handlung, die rechtswidrig ein fremdes Recht beeinträchtigt."]),
  n(["remedy_legal", "der Rechtsbehelf", "средство правовой защиты", "legal remedy", "Процедура для обжалования или устранения нарушения.", "A procedure for challenging or correcting a violation.", "Ein Verfahren zur Anfechtung oder Beseitigung einer Verletzung."]),
  n(["enforcement_mechanism", "der Durchsetzungsmechanismus", "механизм обеспечения исполнения", "enforcement mechanism", "Способ добиться выполнения принятого правила.", "A means of ensuring compliance with an adopted rule.", "Ein Mittel, um die Einhaltung einer beschlossenen Regel sicherzustellen."]),
  n(["social_cohesion", "der gesellschaftliche Zusammenhalt", "общественная сплочённость", "social cohesion", "Связи и доверие, удерживающие общество вместе.", "The ties and trust that hold a society together.", "Die Bindungen und das Vertrauen, die eine Gesellschaft zusammenhalten."]),
  n(["demographic_change", "der demografische Wandel", "демографические изменения", "demographic change", "Долговременное изменение состава и возраста населения.", "Long-term change in the composition and age of a population.", "Die langfristige Veränderung von Zusammensetzung und Alter einer Bevölkerung."]),
  n(["social_mobility", "die soziale Mobilität", "социальная мобильность", "social mobility", "Возможность изменить своё социально-экономическое положение.", "The possibility of changing one's socioeconomic position.", "Die Möglichkeit, die eigene soziale und wirtschaftliche Stellung zu verändern."]),
  n(["educational_inequality", "die Bildungsungleichheit", "неравенство в образовании", "educational inequality", "Неравный доступ к качественному образованию.", "Unequal access to high-quality education.", "Der ungleiche Zugang zu hochwertiger Bildung."]),
  n(["labor_exploitation", "die Arbeitsausbeutung", "эксплуатация труда", "labor exploitation", "Несправедливое использование работы людей ради выгоды.", "The unfair use of people's work for gain.", "Die ungerechte Nutzung der Arbeit von Menschen zum Vorteil anderer."]),
  n(["conflict_of_interest", "der Interessenkonflikt", "конфликт интересов", "conflict of interest", "Ситуация, когда личная выгода может повлиять на обязанность.", "A situation in which private benefit may influence a duty.", "Eine Situation, in der ein persönlicher Vorteil eine Pflicht beeinflussen kann."]),
  n(["whistleblowing", "die Meldung von Missständen", "сообщение о нарушениях", "whistleblowing", "Информирование о серьёзных нарушениях внутри организации.", "Reporting serious wrongdoing within an organization.", "Das Melden schwerwiegender Missstände innerhalb einer Organisation."]),
  n(["confidentiality", "die Vertraulichkeit", "конфиденциальность", "confidentiality", "Обязанность не раскрывать защищённые сведения.", "The duty not to disclose protected information.", "Die Pflicht, geschützte Informationen nicht offenzulegen."]),
  n(["informed_consent", "die informierte Einwilligung", "информированное согласие", "informed consent", "Согласие после получения понятной информации о рисках.", "Consent given after receiving clear information about risks.", "Eine Einwilligung nach verständlicher Information über Risiken."]),
  n(["ethical_dilemma", "das ethische Dilemma", "этическая дилемма", "ethical dilemma", "Выбор между решениями, каждое из которых имеет моральную цену.", "A choice between options each carrying a moral cost.", "Eine Wahl zwischen Möglichkeiten, von denen jede einen moralischen Preis hat."]),
  n(["collective_bargaining", "die Tarifverhandlung", "коллективные переговоры", "collective bargaining", "Переговоры работников и работодателей об условиях труда.", "Negotiations between workers and employers about working conditions.", "Verhandlungen zwischen Arbeitnehmern und Arbeitgebern über Arbeitsbedingungen."]),
  n(["civic_engagement", "das bürgerschaftliche Engagement", "гражданская вовлечённость", "civic engagement", "Участие людей в решении общественных вопросов.", "People's participation in addressing public issues.", "Die Beteiligung von Menschen an der Lösung öffentlicher Fragen."]),

  // Culture, language and intellectual analysis
  n(["cultural_heritage", "das Kulturerbe", "культурное наследие", "cultural heritage", "Ценности и традиции, передаваемые следующим поколениям.", "Values and traditions passed on to future generations.", "Werte und Traditionen, die an künftige Generationen weitergegeben werden."]),
  n(["collective_memory", "das kollektive Gedächtnis", "коллективная память", "collective memory", "Общее представление общества о прошлом.", "A society's shared understanding of its past.", "Das gemeinsame Verständnis einer Gesellschaft von ihrer Vergangenheit."]),
  n(["cultural_appropriation", "die kulturelle Aneignung", "культурное присвоение", "cultural appropriation", "Использование элементов чужой культуры без должного контекста или уважения.", "Use of elements of another culture without proper context or respect.", "Die Verwendung von Elementen einer anderen Kultur ohne angemessenen Kontext oder Respekt."]),
  n(["narrative", "das Deutungsmuster", "интерпретационный нарратив", "interpretive narrative", "Способ связывать события в осмысленную историю.", "A way of linking events into a meaningful account.", "Eine Art, Ereignisse zu einer sinnvollen Geschichte zu verbinden."]),
  n(["rhetoric", "die Rhetorik", "риторика", "rhetoric", "Использование языка для убеждения аудитории.", "The use of language to persuade an audience.", "Der Einsatz von Sprache zur Überzeugung eines Publikums."]),
  n(["connotation", "die Nebenbedeutung", "коннотация", "connotation", "Дополнительный эмоциональный или культурный смысл слова.", "An additional emotional or cultural meaning of a word.", "Eine zusätzliche emotionale oder kulturelle Bedeutung eines Wortes."]),
  n(["terminology", "die Fachterminologie", "профессиональная терминология", "specialized terminology", "Набор специальных слов определённой области.", "The set of specialized words used in a field.", "Die Gesamtheit der Fachwörter eines Gebiets."]),
  n(["interpretive_scope", "der Deutungsspielraum", "пространство интерпретации", "scope for interpretation", "Диапазон допустимых трактовок текста или нормы.", "The range of permissible interpretations of a text or rule.", "Der Bereich zulässiger Auslegungen eines Textes oder einer Regel."]),
  n(["representation", "die Repräsentation", "репрезентация", "representation", "То, как группа или явление изображается в культуре и СМИ.", "The way a group or phenomenon is depicted in culture and media.", "Die Art, wie eine Gruppe oder ein Phänomen in Kultur und Medien dargestellt wird."]),
  n(["stereotype", "das Stereotyp", "стереотип", "stereotype", "Упрощённое устойчивое представление о группе людей.", "A simplified fixed belief about a group of people.", "Eine vereinfachte feste Vorstellung über eine Gruppe von Menschen."]),
  n(["credibility", "die Glaubwürdigkeit", "достоверность источника", "credibility", "Степень доверия к источнику или утверждению.", "The degree to which a source or claim can be believed.", "Das Maß, in dem einer Quelle oder Behauptung vertraut werden kann."]),
  n(["verification_process", "das Prüfverfahren", "процедура проверки", "verification procedure", "Последовательность действий для подтверждения достоверности.", "A sequence of actions for confirming reliability.", "Eine Abfolge von Handlungen zur Bestätigung der Zuverlässigkeit."]),
  n(["public_broadcasting", "der öffentlich-rechtliche Rundfunk", "общественное вещание", "public broadcasting", "Медиаслужба, выполняющая общественную задачу.", "A media service carrying out a public mission.", "Ein Mediendienst, der einen öffentlichen Auftrag erfüllt."]),
  n(["freedom_of_expression", "die Meinungsfreiheit", "свобода выражения мнения", "freedom of expression", "Право выражать взгляды без незаконного подавления.", "The right to express views without unlawful suppression.", "Das Recht, Ansichten ohne rechtswidrige Unterdrückung zu äußern."]),
  n(["censorship", "die Zensur", "цензура", "censorship", "Контроль или подавление публикации информации и мнений.", "Control or suppression of the publication of information and views.", "Die Kontrolle oder Unterdrückung der Veröffentlichung von Informationen und Meinungen."]),

  // High-level actions
  v(["assess_implications", "Auswirkungen abschätzen", "оценивать последствия", "assess implications", "Определять вероятные последствия решения.", "To determine the likely consequences of a decision.", "Die wahrscheinlichen Folgen einer Entscheidung bestimmen."]),
  v(["challenge_assumption", "eine Annahme hinterfragen", "ставить предпосылку под сомнение", "challenge an assumption", "Критически проверять то, что принималось за исходное.", "To critically examine something previously taken for granted.", "Etwas kritisch prüfen, das zuvor als gegeben galt."]),
  v(["clarify_distinction", "eine Unterscheidung verdeutlichen", "прояснять различие", "clarify a distinction", "Делать разницу между понятиями понятной.", "To make the difference between concepts clear.", "Den Unterschied zwischen Begriffen verständlich machen."]),
  v(["comply_with_law", "gesetzliche Vorgaben befolgen", "соблюдать законодательные нормы", "comply with legislation", "Действовать в соответствии с требованиями закона.", "To act in accordance with legal requirements.", "Gemäß gesetzlichen Anforderungen handeln."]),
  v(["convey_meaning", "eine Bedeutung vermitteln", "передавать смысл", "convey meaning", "Доносить значение или идею до адресата.", "To communicate a meaning or idea to an audience.", "Eine Bedeutung oder Idee an einen Adressaten weitergeben."]),
  v(["detect_bias", "eine Verzerrung erkennen", "выявлять смещение", "detect bias", "Обнаруживать систематическое искажение оценки.", "To identify systematic distortion in an assessment.", "Eine systematische Verzerrung einer Bewertung feststellen."]),
  v(["establish_causality", "Kausalität nachweisen", "устанавливать причинность", "establish causality", "Доказывать, что одно явление вызывает другое.", "To prove that one phenomenon causes another.", "Nachweisen, dass ein Phänomen ein anderes verursacht."]),
  v(["foster_dialogue", "einen Dialog fördern", "содействовать диалогу", "foster dialogue", "Создавать условия для конструктивного разговора.", "To create conditions for constructive discussion.", "Bedingungen für ein konstruktives Gespräch schaffen."]),
  v(["grant_authority", "eine Befugnis erteilen", "предоставлять полномочие", "grant authority", "Официально давать право принимать решения.", "To officially give the right to make decisions.", "Offiziell das Recht geben, Entscheidungen zu treffen."]),
  v(["hold_accountable", "zur Verantwortung ziehen", "привлекать к ответственности", "hold accountable", "Требовать ответа за действия и последствия.", "To require someone to answer for actions and consequences.", "Von jemandem Rechenschaft für Handlungen und Folgen verlangen."]),
  v(["identify_shortcomings", "Mängel aufzeigen", "выявлять недостатки", "identify shortcomings", "Показывать слабые стороны подхода или результата.", "To show weaknesses in an approach or result.", "Schwächen eines Ansatzes oder Ergebnisses sichtbar machen."]),
  v(["justify_intervention", "einen Eingriff rechtfertigen", "обосновывать вмешательство", "justify an intervention", "Приводить достаточные основания для вмешательства.", "To provide sufficient reasons for intervening.", "Ausreichende Gründe für einen Eingriff anführen."]),
  v(["mobilize_resources", "Ressourcen mobilisieren", "мобилизовать ресурсы", "mobilize resources", "Направлять необходимые средства на достижение цели.", "To direct necessary means toward achieving a goal.", "Notwendige Mittel auf das Erreichen eines Ziels ausrichten."]),
  v(["preserve_autonomy", "Autonomie wahren", "сохранять автономию", "preserve autonomy", "Защищать способность самостоятельно принимать решения.", "To protect the ability to make decisions independently.", "Die Fähigkeit schützen, selbstständig Entscheidungen zu treffen."]),
  v(["reconsider_position", "eine Position überdenken", "пересматривать позицию", "reconsider a position", "Заново оценивать взгляд с учётом новых оснований.", "To reassess a view in light of new grounds.", "Eine Sichtweise angesichts neuer Gründe erneut bewerten."]),
  v(["regulate_market", "einen Markt regulieren", "регулировать рынок", "regulate a market", "Устанавливать правила функционирования рынка.", "To set rules for how a market functions.", "Regeln für die Funktionsweise eines Marktes festlegen."]),
  v(["resolve_dispute", "einen Streit beilegen", "урегулировать спор", "resolve a dispute", "Завершать конфликт через решение или договорённость.", "To end a conflict through a decision or agreement.", "Einen Konflikt durch Entscheidung oder Vereinbarung beenden."]),
  v(["safeguard_rights", "Rechte gewährleisten", "гарантировать права", "safeguard rights", "Обеспечивать защиту прав от нарушения.", "To ensure protection of rights from violation.", "Den Schutz von Rechten vor Verletzungen sicherstellen."]),
  v(["stimulate_debate", "eine Debatte anstoßen", "инициировать дискуссию", "stimulate debate", "Запускать содержательное обсуждение вопроса.", "To initiate substantial discussion of an issue.", "Eine inhaltliche Diskussion einer Frage beginnen."]),
  v(["uphold_principle", "einen Grundsatz wahren", "отстаивать принцип", "uphold a principle", "Сохранять верность важному правилу несмотря на давление.", "To remain faithful to an important rule despite pressure.", "Einem wichtigen Grundsatz trotz Druck treu bleiben."]),

  // Nuanced evaluation
  a(["ambivalent", "zwiespältig", "двойственный", "ambivalent", "Сочетающий противоположные оценки или чувства.", "Combining opposing assessments or feelings.", "Gegensätzliche Bewertungen oder Gefühle verbindend."]),
  a(["authoritative", "maßgeblich", "авторитетный", "authoritative", "Имеющий значительный вес благодаря компетентности.", "Carrying substantial weight because of expertise.", "Aufgrund von Fachkenntnis erhebliches Gewicht besitzend."]),
  a(["comprehensive", "umfassend", "всеобъемлющий", "comprehensive", "Охватывающий все существенные аспекты.", "Covering all essential aspects.", "Alle wesentlichen Aspekte umfassend."]),
  a(["conclusive", "abschließend beweiskräftig", "окончательно доказательный", "conclusive", "Достаточный для окончательного вывода.", "Sufficient to establish a final conclusion.", "Ausreichend, um eine endgültige Schlussfolgerung zu begründen."]),
  a(["credible_source", "glaubhaft", "заслуживающий доверия", "credible", "Способный вызвать доверие благодаря подтверждениям.", "Able to inspire belief because of supporting evidence.", "Aufgrund stützender Belege Vertrauen erweckend."]),
  a(["deficient", "mangelhaft", "имеющий существенные недостатки", "deficient", "Не отвечающий необходимому уровню качества.", "Failing to meet a necessary standard of quality.", "Einen notwendigen Qualitätsstandard nicht erfüllend."]),
  a(["equitable", "gerecht ausgestaltet", "обеспечивающий справедливость", "equitable", "Учитывающий потребности сторон справедливым образом.", "Taking the needs of parties into account fairly.", "Die Bedürfnisse der Beteiligten in gerechter Weise berücksichtigend."]),
  a(["irreversible", "unumkehrbar", "необратимый", "irreversible", "Такой, последствия которого нельзя отменить.", "Having consequences that cannot be reversed.", "Mit Folgen, die nicht rückgängig gemacht werden können."]),
  a(["legitimate_adjective", "rechtmäßig begründet", "легитимно обоснованный", "legitimate", "Основанный на признанном праве или убедительной причине.", "Based on recognized authority or a convincing reason.", "Auf anerkannter Befugnis oder überzeugendem Grund beruhend."]),
  a(["multifaceted", "vielschichtig", "многоаспектный", "multifaceted", "Имеющий несколько взаимосвязанных сторон.", "Having several interconnected aspects.", "Mehrere miteinander verbundene Aspekte besitzend."]),
  a(["negligible", "vernachlässigbar", "незначительный", "negligible", "Настолько малый, что почти не влияет на вывод.", "So small that it scarcely affects the conclusion.", "So gering, dass es die Schlussfolgerung kaum beeinflusst."]),
  a(["pervasive", "allgegenwärtig", "всепроникающий", "pervasive", "Распространённый во многих областях и трудно избегаемый.", "Widespread across many areas and difficult to avoid.", "In vielen Bereichen verbreitet und schwer zu vermeiden."]),
  a(["reciprocal", "wechselseitig", "взаимный", "reciprocal", "Действующий одинаково в обоих направлениях.", "Operating in the same way in both directions.", "In gleicher Weise in beiden Richtungen wirkend."]),
  a(["sustainable_longrun", "dauerhaft tragfähig", "устойчивый в долгой перспективе", "sustainable in the long run", "Способный продолжаться без разрушения ресурсов или основы.", "Able to continue without destroying resources or its foundation.", "Fortsetzbar, ohne Ressourcen oder Grundlage zu zerstören."]),
  a(["unprecedented", "beispiellos", "беспрецедентный", "unprecedented", "Не имевший аналогов в предыдущем опыте.", "Having no equivalent in previous experience.", "Ohne Entsprechung in der bisherigen Erfahrung."])
];
