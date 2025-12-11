using System;

namespace last_project
{
    public static class Session
    {
        // 기본 정보
        public static string UserId { get; set; } = "";
        public static string UserName { get; set; } = "";
        public static string Nickname { get; set; } = "";
        public static string Role { get; set; } = "STAFF";

        // 상세 정보
        public static string Email { get; set; } = "";
        public static string Phone { get; set; } = "";
        public static string Birthdate { get; set; } = "";

        // 시스템 정보
        public static DateTime LoginTime { get; set; } = DateTime.Now;

        // ★ [추가됨] 프로필 사진 경로 저장 변수
        public static string ProfileImagePath { get; set; } = "";

        // 데이터 세팅 함수 (이미지 경로 매개변수 추가됨)
        public static void SetUser(string id, string name, string nickname, string role, string email, string phone, string birthdate, string profileImagePath = "")
        {
            UserId = id;
            UserName = name;
            Nickname = nickname;
            Role = role;
            Email = email;
            Phone = phone;
            Birthdate = birthdate;

            // ★ 이미지 경로 저장 (없으면 빈 값)
            ProfileImagePath = profileImagePath;

            LoginTime = DateTime.Now;
        }
    }
}