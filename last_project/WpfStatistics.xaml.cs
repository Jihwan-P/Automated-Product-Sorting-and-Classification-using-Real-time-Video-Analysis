using System;

using System.Collections.Generic;

using System.Windows.Controls;

using LiveChartsCore;

using LiveChartsCore.SkiaSharpView;

using LiveChartsCore.SkiaSharpView.Painting;

using SkiaSharp; // [필수]



namespace last_project

{

    public partial class WpfStatistics : System.Windows.Controls.UserControl

    {

        // --- 차트 데이터 속성들 ---

        public ISeries[] SeriesA { get; set; }

        public ISeries[] SeriesB { get; set; }

        public ISeries[] CategorySeries { get; set; }

        public ISeries[] HourlySeries { get; set; }

        public ISeries[] InOutSeries { get; set; }

        public ISeries[] AiGaugeSeries { get; set; }

        public ISeries[] TimeSeries { get; set; }

        public ISeries[] BrandSeries { get; set; }

        public ISeries[] CabinetSeries { get; set; }

        public ISeries[] CommandSeries { get; set; }



        // --- 축 설정 ---

        public Axis[] HourXAxes { get; set; }

        public Axis[] DateXAxes { get; set; }

        public Axis[] TaskXAxes { get; set; }

        public Axis[] BrandXAxes { get; set; }

        public Axis[] CabinetXAxes { get; set; }

        public Axis[] CommandYAxes { get; set; }

        public Axis[] CommonYAxes { get; set; }



        // 툴팁 폰트 설정

        public SolidColorPaint TooltipTextPaint { get; set; }

        public SolidColorPaint TooltipBackgroundPaint { get; set; }


        public WpfStatistics()

        {

            InitializeComponent();



            // 1. 한글 폰트(맑은 고딕)

            var koreanTypeface = SKTypeface.FromFamilyName("Malgun Gothic");



            // 2. 축 라벨용 페인트 (회색)

            var grayTextPaint = new SolidColorPaint

            {

                Color = SKColors.Gray,

                SKTypeface = koreanTypeface

            };



            // 3. 축 라벨용 페인트 (흰색)

            var whiteTextPaint = new SolidColorPaint

            {

                Color = SKColors.White,

                SKTypeface = koreanTypeface

            };



            // 4. [수정 완료] 툴팁용 페인트 (진한 검은색 계열)

            // 배경이 밝은 회색이므로 글씨는 진해야 잘 보입니다!

            TooltipTextPaint = new SolidColorPaint

            {

                Color = SKColors.White, // 진한 회색 (거의 검정)

                SKTypeface = koreanTypeface

            };

            TooltipBackgroundPaint = new SolidColorPaint
            {
                Color = new SKColor(45, 45, 48, 230) // R, G, B, Alpha(투명도: 0~255)
            };

            // ---------------------------------------------------------

            // 데이터 설정

            // ---------------------------------------------------------



            // 1. A 수납장

            SeriesA = new ISeries[]

            {

                new PieSeries<double> { Values = new double[] { 50 }, Name = "사용 중", InnerRadius = 60, Fill = new SolidColorPaint(SKColors.DodgerBlue) },

                new PieSeries<double> { Values = new double[] { 50 }, Name = "빈 공간", InnerRadius = 60, Fill = new SolidColorPaint(new SKColor(40, 40, 40)) }

            };



            // 2. B 수납장

            SeriesB = new ISeries[]

            {

                new PieSeries<double> { Values = new double[] { 100 }, Name = "사용 중", InnerRadius = 60, Fill = new SolidColorPaint(SKColors.HotPink) },

                new PieSeries<double> { Values = new double[] { 0 }, Name = "빈 공간", InnerRadius = 60, Fill = new SolidColorPaint(new SKColor(40, 40, 40)) }

            };



            // 3. 브랜드 비율 (빈폴, 퓨마, 데상트, 엄브로)

            // - Values: 실제 재고 수량을 입력 (그래프 크기에 반영됨)

            // - ToolTipLabelFormatter: 마우스 오버 시 표시할 텍스트 설정

            CategorySeries = new ISeries[]

            {



                new PieSeries<double> {

                    Values = new double[] { 1 }, // 실제 수량 입력 -> 크기가 제일 커짐

                    Name = "빈폴",

                    InnerRadius = 40,

                    Fill = new SolidColorPaint(SKColors.ForestGreen),

                    // [핵심] 툴팁 설정: "이름: 수량개" 형태로 표시

                    ToolTipLabelFormatter = point => $"{point.Context.Series.Name}: {point.PrimaryValue}개"

                },





                new PieSeries<double> {

                    Values = new double[] { 0 },

                    Name = "엄브로",

                    InnerRadius = 40,

                    Fill = new SolidColorPaint(SKColors.Crimson),

                    ToolTipLabelFormatter = point => $"{point.Context.Series.Name}: {point.PrimaryValue}개"

                },





                new PieSeries<double> {

                    Values = new double[] { 1 },

                    Name = "퓨마",

                    InnerRadius = 40,

                    Fill = new SolidColorPaint(SKColors.DimGray),

                    ToolTipLabelFormatter = point => $"{point.Context.Series.Name}: {point.PrimaryValue}개"

                },



                // 4. 엄브로 (재고 10개 가정)

                new PieSeries<double> {

                    Values = new double[] { 1}, // 수량이 적으므로 크기가 제일 작아짐

                    Name = "데상트",

                    InnerRadius = 40,

                    Fill = new SolidColorPaint(SKColors.RoyalBlue),

                    ToolTipLabelFormatter = point => $"{point.Context.Series.Name}: {point.PrimaryValue}개"

                }

            };

            // 4. 시간대별

            HourlySeries = new ISeries[] { new ColumnSeries<double> { Values = new double[] { 5, 12, 25, 18, 10, 4 }, Name = "건수", Fill = new SolidColorPaint(SKColors.MediumPurple) } };

            HourXAxes = new Axis[] { new Axis { Labels = new[] { "09시", "11시", "13시", "15시", "17시", "19시" }, LabelsPaint = grayTextPaint } };



            // 5. 입출고

            InOutSeries = new ISeries[] {

                new LineSeries<double> { Values = new double[] { 1,5 }, Name = "입고", Stroke = new SolidColorPaint(SKColors.Cyan) { StrokeThickness = 3 }, Fill = null, GeometrySize = 10 },

                new LineSeries<double> { Values = new double[] { 1,4 }, Name = "출고", Stroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 3 }, Fill = null, GeometrySize = 10 }

            };

            DateXAxes = new Axis[] { new Axis { Labels = new[] {  "12/9", "12/10" }, LabelsPaint = grayTextPaint } };



            // 6. AI 인식률

            AiGaugeSeries = new ISeries[] {

                new PieSeries<double> { Values = new double[] { 98.5 }, Name = "성공", InnerRadius = 80, Fill = new SolidColorPaint(SKColors.SpringGreen) },

                new PieSeries<double> { Values = new double[] { 1.5 }, Name = "실패", InnerRadius = 80, Fill = new SolidColorPaint(new SKColor(40, 40, 40)) }

            };



            // 7. 평균 작업 시간

            TimeSeries = new ISeries[] { new ColumnSeries<double> { Values = new double[] { 45, 42 }, Name = "시간(초)", Fill = new SolidColorPaint(SKColors.MediumSlateBlue) } };

            TaskXAxes = new Axis[] { new Axis { Labels = new[] { "적재", "출고" }, LabelsPaint = grayTextPaint } };



            // 8. 브랜드별 재고 (누적) - [수정됨]
            BrandSeries = new ISeries[] {
                new StackedColumnSeries<double> {
                    Values = new double[] { 1, 0, 0, 0 },
                    Name = "창고 A",
                    StackGroup = 0,
                    Fill = new SolidColorPaint(SKColors.DodgerBlue),
                    // [핵심 1] 막대 내부 여백을 늘려서 -> 막대를 얇게 만들고 사이 간격을 넓힘
                    Padding = 10,
                    MaxBarWidth = double.PositiveInfinity
                },
                new StackedColumnSeries<double> {
                    Values = new double[] { 0, 0, 1, 1 },
                    Name = "창고 B",
                    StackGroup = 0,
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                    Padding = 10  // 위와 동일하게 설정해야 줄이 맞습니다
                }
            };

            // [핵심 2] 회전을 끄고, 글자 크기를 조절하여 간격 확보
            BrandXAxes = new Axis[] {
                new Axis {
                    Labels = new[] { "빈폴", "엄브로", "퓨마", "데상트" },
                    LabelsPaint = grayTextPaint,
                    TextSize = 12,        // 글자 크기를 조금 줄임 (기본값 약 16 -> 12)
                    MinStep = 1,
                    ForceStepToMin = true,
                    LabelsRotation = 0    // 회전 없음 (평평하게)
                }
            };


            // 9. 수납장별 브랜드 현황 (수정됨)
            // Values = new double[] { A수납장 개수, B수납장 개수 }
            CabinetSeries = new ISeries[] {
                // 1. 빈폴 (A: 1개, B: 0개)
                new ColumnSeries<double> {
                    Name = "빈폴",
                    Values = new double[] { 1, 0 },
                    Fill = new SolidColorPaint(SKColors.ForestGreen)
                },
                
                // 2. 퓨마 (A: 1개, B: 0개)
                new ColumnSeries<double> {
                    Name = "엄브로",
                    Values = new double[] { 0, 0 },
                    Fill = new SolidColorPaint(SKColors.Crimson)
                },

                // 3. 데상트 (A: 0개, B: 1개)

                 new ColumnSeries<double> {
                    Name = "퓨마",
                    Values = new double[] { 0, 1 },
                    Fill = new SolidColorPaint(SKColors.RoyalBlue)
                 },
              

                // 4. 엄브로 (A: 0개, B: 0개)

                   new ColumnSeries<double> {
                    Name = "데상트",
                    Values = new double[] { 0, 1 },
                    Fill = new SolidColorPaint(SKColors.DimGray)
                  },

            };

            // X축 라벨 설정 (그대로 유지)
            CabinetXAxes = new Axis[] {
                new Axis {
                    Labels = new[] { "A 수납장", "B 수납장" },
                    LabelsPaint = grayTextPaint
                }
            };

            // 10. 명령어 빈도
            CommandSeries = new ISeries[] {
                new RowSeries<double> {
                    Values = new double[] { 50, 35, 12 },
                    Name = "횟수",
                    Fill = new SolidColorPaint(SKColors.LimeGreen),
                    // [수정] 오류가 발생한 ToolTipLabelFormatter 라인을 삭제했습니다.
                    // 기본적으로 마우스를 올리면 숫자가 나옵니다.
                    XToolTipLabelFormatter = point => $"{point.PrimaryValue}회"
                }
            };

            // (축 설정은 그대로 유지)
            CommandYAxes = new Axis[] {
                new Axis {
                    Labels = new[] { "전진(F)", "후진(B)", "정지(S)" },
                    LabelsPaint = whiteTextPaint,
                    MinStep = 1,
                    ForceStepToMin = true
                }
            };

            CommonYAxes = new Axis[] { new Axis { LabelsPaint = grayTextPaint, SeparatorsPaint = new SolidColorPaint(new SKColor(60, 60, 60)) } };



            this.DataContext = this;

        }

    }

}

