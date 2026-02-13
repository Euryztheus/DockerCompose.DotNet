executing user must be in docker group!

entrypoint doesnt support entrypoint list: ["/bin/sh","-c", "..."]
only this -> entrypoint: "/bin/bash -c 'echo hi'" 