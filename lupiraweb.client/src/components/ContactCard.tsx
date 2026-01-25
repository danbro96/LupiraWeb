import EmailIcon from "./icons/EmailIcon";
import PhoneIcon from "./icons/PhoneIcon";
import GitHubIcon from "./icons/GitHubIcon";
import LinkedInIcon from "./icons/LinkedInIcon";
import Link from "./Link";

export default function ContactCard() {
  return (
      <div className="space-y-1 text-sm">
        <div className="flex items-center">
          <EmailIcon className="w-4 h-4 mr-2 text-slate-400" />
          <Link href="mailto:daniel.brostrom@hotmail.se">
            daniel.brostrom@hotmail.se
          </Link>
        </div>
        <div className="flex items-center">
          <PhoneIcon className="w-4 h-4 mr-2 text-slate-400" />
          <Link href="tel:+46735028811">+46 (0)73 502 88 11</Link>
        </div>
        <div className="flex items-center">
          <GitHubIcon className="w-4 h-4 mr-2 text-slate-400" />
          <Link href="https://github.com/danbro96">danbro96</Link>
        </div>
        <div className="flex items-center">
          <LinkedInIcon className="w-4 h-4 mr-2 text-slate-400" />
          <Link href="https://www.linkedin.com/in/daniel-brostrom">
            Daniel Broström
          </Link>
        </div>
      </div>
  );
}
