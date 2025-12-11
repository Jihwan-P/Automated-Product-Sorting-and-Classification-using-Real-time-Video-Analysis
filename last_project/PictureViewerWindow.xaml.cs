using System;
using System.Windows;
using System.Windows.Input; // MouseButtonEventArgs를 위해 추가
using System.Windows.Media.Imaging; // BitmapImage를 위해 추가

namespace last_project
{
    public partial class PictureViewerWindow : Window
    {
        /// <summary>
        /// (수정) 기본 생성자 대신, 이미지 경로를 받는 생성자를 만듭니다.
        /// </summary>
        /// <param name="imagePath">표시할 이미지의 파일 경로</param>
        public PictureViewerWindow(string imagePath)
        {
            InitializeComponent();

            try
            {
                // (핵심) 전달받은 경로로 이미지를 로드합니다.
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(imagePath, UriKind.Absolute); // 절대 경로
                image.CacheOption = BitmapCacheOption.OnLoad; // 파일 락 방지
                image.EndInit();

                // XAML의 <Image x:Name="BigImage">에 연결
                BigImage.Source = image;
            }
            catch (Exception ex)
            {
                // 이미지 로드 실패 시
                System.Windows.MessageBox.Show($"이미지를 로드할 수 없습니다: {imagePath}\n오류: {ex.Message}");
                this.Close(); // 창 닫기
            }
        }

        /// <summary>
        /// (추가) XAML의 Grid에서 연결한 클릭 이벤트입니다.
        /// </summary>
        private void Close_Click(object? sender, MouseButtonEventArgs e)
        {
            this.Close(); // 창 닫기
        }
    }
}