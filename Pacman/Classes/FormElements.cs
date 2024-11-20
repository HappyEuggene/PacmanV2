using System.Windows.Forms;

namespace Pacman
{
    public class FormElements
    {
        public Label PlayerOneScoreText = new Label();

        public void CreateFormScoreLabel(Form formInstance)
        {
            PlayerOneScoreText.ForeColor = System.Drawing.Color.White;
            PlayerOneScoreText.Font = new System.Drawing.Font("Folio XBd BT", 14);
            PlayerOneScoreText.Top = 5;
            PlayerOneScoreText.Left = 20;
            PlayerOneScoreText.Height = 20;
            PlayerOneScoreText.Width = 100;
            PlayerOneScoreText.Text = "SCORE";
            formInstance.Controls.Add(PlayerOneScoreText);
        }
    }
}
