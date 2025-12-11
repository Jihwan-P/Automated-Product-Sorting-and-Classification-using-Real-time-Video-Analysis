using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using System.Text.Json; // JSON 파싱용

namespace last_project
{
    public partial class WpfSlotEditor : System.Windows.Controls.UserControl
    {
        private const string CAM1_URL = "http://192.168.0.79:8000/stream.mjpg";
        private const string CAM2_URL = "http://192.168.0.112:8000/stream.mjpg";
        private const string CAM3_URL = "http://192.168.0.10:8000/stream.mjpg";

        public event EventHandler<SlotDrawnEventArgs>? SlotDrawn;

        public WpfSlotEditor()
        {
            InitializeComponent();
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                await SlotWebView.EnsureCoreWebView2Async(null);

                // 1. 자바스크립트에서 보낸 좌표 메시지 수신 연결
                SlotWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

                // 2. 페이지 로딩 시 스크립트 주입
                await InjectDrawingScript();

                SlotWebView.CoreWebView2.Navigate(CAM1_URL);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"웹뷰 초기화 실패: {ex.Message}");
            }
        }

        private async Task InjectDrawingScript()
        {
            // 0.5초마다 체크하여 드래그용 투명판(drag-layer) 복구
            string script = @"
                function initDrawingLayer() {
                    if (document.getElementById('drag-layer')) return;

                    const style = document.createElement('style');
                    style.innerHTML = `
                        body { margin: 0; padding: 0; overflow: hidden; background: black; user-select: none; }
                        img { width: 100%; height: 100%; object-fit: fill; display: block; -webkit-user-drag: none; pointer-events: none; }
                        
                        #drag-layer { 
                            position: absolute; top: 0; left: 0; width: 100%; height: 100%; 
                            z-index: 9999; cursor: crosshair; 
                        }
                        
                        #selection-box { 
                            position: absolute; 
                            border: 2px solid red; 
                            background-color: rgba(255, 0, 0, 0.3); 
                            display: none; 
                            pointer-events: none; /* ★핵심 수정★: 박스가 마우스 이벤트를 무시하게 함 (떨림 방지) */
                        }
                    `;
                    document.head.appendChild(style);

                    const dragLayer = document.createElement('div');
                    dragLayer.id = 'drag-layer';
                    document.body.appendChild(dragLayer);

                    const box = document.createElement('div');
                    box.id = 'selection-box';
                    dragLayer.appendChild(box);

                    let isDrawing = false;
                    let startX = 0, startY = 0;

                    dragLayer.addEventListener('mousedown', (e) => {
                        if (e.button !== 0) return;
                        isDrawing = true;
                        startX = e.offsetX;
                        startY = e.offsetY;
                        
                        box.style.left = startX + 'px';
                        box.style.top = startY + 'px';
                        box.style.width = '0px';
                        box.style.height = '0px';
                        box.style.display = 'block';
                        
                        e.preventDefault();
                    });

                    dragLayer.addEventListener('mousemove', (e) => {
                        if (!isDrawing) return;
                        const currentX = e.offsetX;
                        const currentY = e.offsetY;

                        const width = Math.abs(currentX - startX);
                        const height = Math.abs(currentY - startY);
                        const left = Math.min(currentX, startX);
                        const top = Math.min(currentY, startY);

                        box.style.width = width + 'px';
                        box.style.height = height + 'px';
                        box.style.left = left + 'px';
                        box.style.top = top + 'px';
                    });

                    dragLayer.addEventListener('mouseup', () => {
                        if (!isDrawing) return;
                        isDrawing = false;
                        
                        const rectData = {
                            x: parseFloat(box.style.left),
                            y: parseFloat(box.style.top),
                            w: parseFloat(box.style.width),
                            h: parseFloat(box.style.height)
                        };
                        
                        if (rectData.w > 5 && rectData.h > 5) {
                            window.chrome.webview.postMessage(JSON.stringify(rectData));
                        }
                        
                        box.style.display = 'none';
                    });

                    window.addEventListener('contextmenu', e => e.preventDefault());
                }

                setInterval(initDrawingLayer, 500);
                initDrawingLayer();
            ";

            await SlotWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);
        }

        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var jsonString = e.TryGetWebMessageAsString();
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    return;
                }

                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    JsonElement root = doc.RootElement;
                    double x = root.GetProperty("x").GetDouble();
                    double y = root.GetProperty("y").GetDouble();
                    double w = root.GetProperty("w").GetDouble();
                    double h = root.GetProperty("h").GetDouble();

                    SlotDrawn?.Invoke(this, new SlotDrawnEventArgs(x, y, w, h));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"좌표 수신 오류: {ex.Message}");
            }
        }

        private void CamSelector_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (SlotWebView?.CoreWebView2 == null) return;
            if (CamSelector.SelectedItem is ComboBoxItem item)
            {
                string url = CAM1_URL;
                string? tag = item.Tag as string;
                if (tag == "cam2") url = CAM2_URL;
                else if (tag == "cam3") url = CAM3_URL;
                SlotWebView.CoreWebView2.Navigate(url);
            }
        }
    }

    public class SlotDrawnEventArgs : EventArgs
    {
        public double X { get; }
        public double Y { get; }
        public double W { get; }
        public double H { get; }
        public SlotDrawnEventArgs(double x, double y, double w, double h)
        {
            X = x; Y = y; W = w; H = h;
        }
    }

}
