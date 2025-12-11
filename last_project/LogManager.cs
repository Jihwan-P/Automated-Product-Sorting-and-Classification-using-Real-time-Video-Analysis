using System;
using System.Collections.Generic;
using System.Linq;

namespace last_project
{
    /// <summary>
    /// 애플리케이션 전역에서 로그를 수집하고 관리하는 static 클래스입니다.
    /// (partial: 혹시 다른 곳에 LogManager가 정의되어 있어도 충돌하지 않도록)
    /// </summary>
    public static partial class LogManager // ◀◀◀ partial 키워드 포함!
    {
        // 1. (로그 보관함) 로그 메시지를 순서대로 저장할 리스트입니다.
        private static List<string> logMessages = new List<string>();

        // 2. (스레드 잠금 장치)
        private static readonly object _lock = new object();

        /// <summary>
        /// 새 로그 메시지를 보관함에 추가합니다.
        /// </summary>
        public static void Add(string message)
        {
            string timestampedMessage = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] - {message}";

            lock (_lock)
            {
                logMessages.Add(timestampedMessage);
            }

            System.Diagnostics.Debug.WriteLine(timestampedMessage);
        }

        /// <summary>
        /// 지금까지 기록된 모든 로그를 '복사본'으로 만들어 반환합니다.
        /// </summary>
        public static List<string> GetLogs()
        {
            lock (_lock)
            {
                return new List<string>(logMessages);
            }
        }

        /// <summary>
        /// (선택 사항) 필요시 모든 로그를 삭제하는 함수입니다.
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                logMessages.Clear();
                Add("모든 로그 기록이 초기화되었습니다.");
            }
        }
    }
}