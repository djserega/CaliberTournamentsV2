namespace CaliberTournamentsV2.Models
{
    public enum PickBanKind { map, operators }
    public enum PickBanType { empty, none, ban, pick }
    public enum PickBanMode { bestOf1, bestOf3, bestOf5 }


    public enum MapsTournamentHacking
        { КараванСарай, ТорговыйЦентр, Деревня, РезиденцияЭмира, ПальмоваяДорога, Больница, ГаваньАмаль, ОтельАльМалик, Объект903, Дамба, Переправа, Депо }

    public enum OperatorsAssault 
        { РекрутШтурмовик, Волк, Перун, Ворон, Плут, Кошмар, Рейн, Корсар, Стерлинг, Авангард, Афела, Фаро, Мустанг, Лазутчик, Старкад, Шаовей }
    public enum OperatorsSupport 
        { РекрутПоддержка, Алмаз, Сварог, Спутник, Кит, Пророк, Штерн, Бурбон, Бишоп, Бастион, Хагана, Матадор, Тибет, Зубр, Один, Инчжоу }
    public enum OperatorsMedic 
        { РекрутМедик, Дед, Травник, Бард, Каравай, Микола, Шатц, Монк, Ватсон, Велюр, Шаршерет, Мигель, Багги, Каваль, Фрейр, Яован }
    public enum OperatorsMarksman
        { РекрутСнайпер, Стрелок, Сокол, Комар, Тень, Стилет, Курт, Скаут, Арчер, Вагабонд, Эйма, Диабло, Султан, Бусел, Видар, Цанлун }

    public enum StatisticTypes
        { all, operators, maps, mapsAndOperators }
    public enum StatisticDetailedTypes
        { none, picked, banned }
}
