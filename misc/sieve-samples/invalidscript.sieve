require ["fileinto", "vacation"];
# Move spam to spam folder
if header :contains "X-Spam-Flag" "YES" {
  fileinto "spam";
  # Stop here so that we do not reply on spams
  stop;
}

invalidSieveCommand

vacation
  # Reply at most once a day to a same sender
  :days 1
  :subject "Out of office reply"
  # List of additional recipient addresses which are included in the auto replying.
  # If a mail's recipient is not the envelope recipient and it's not on this list,
  # no vacation reply is sent for it.
  :addresses ["hello@comtoso.con", "foobar@contoso.com"]
"I'm out of office, please contact Joan Doe instead.
Best regards
John Doe";
