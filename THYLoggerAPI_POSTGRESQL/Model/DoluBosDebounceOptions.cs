namespace THYLoggerAPI_POSTGRESQL.Model
{
    public class DoluBosDebounceOptions
    {
        /// <summary>
        /// Bos/Dolu durumunun degismesi icin art arda gelmesi gereken ayni okuma sayisi.
        /// 1 = debounce kapali (her okuma aninda kaydedilir).
        /// </summary>
        public int RequiredConsecutive { get; set; } = 3;
    }
}
