namespace CaliberTournamentsV2.Models
{
    public enum PickBanKind { map, operators }
    public enum PickBanType { none, ban, pick }
    public enum PickBanMode { bestOf1, bestOf3, bestOf5 }


    public enum MapsTournamentHacking
        { КараванСарай, ТорговыйЦентр, Деревня, РезиденцияЭмира, ПальмоваяДорога, Больница, ГаваньАмаль, ОтельАльМалик, Объект903, Дамба, Переправа, Депо }

    // permanent ban: Эйма, багги, сварог, мустанг
    public enum OperatorsAssault 
        { РекрутШтурмовик, Волк, Перун, Ворон, Плут, Кошмар, Рейн, Корсар, Стерлинг, Авангард, Афела, Фаро, Лазутчик }
    public enum OperatorsSupport 
        { РекрутПоддержка, Алмаз, Спутник, Кит, Пророк, Штерн, Бурбон, Бишоп, Бастион, Хагана, Матадор, Тибет, Зубр }
    public enum OperatorsMedic 
        { РекрутМедик, Дед, Травник, Бард, Каравай, Микола, Шатц, Монк, Ватсон, Велюр, Шаршерет, Мигель, Каваль }
    public enum OperatorsMarksman
        { РекрутСнайпер, Стрелок, Сокол, Комар, Тень, Стилет, Курт, Скаут, Арчер, Вагабонд, Диабло, Султан, Бусел }

    public enum StatisticTypes
        { all, operators, maps }
    public enum StatisticDetailedTypes
        { none, picked, banned }
}
