# Candidate

This was a sponsored learning program project with JMU.<br>
This was not just to develop an API but to walk in my customer's shoes as a Site Reliability Engineer.<br>
I wanted to know where in the development process I could integrate tooling and testing in a way that made sense for developers<br>
I found that quality and correctness are normally tested, and checks are run before the PR is merged, but what about unplanned failures?
I wanted to test a tool that would help with that planning. What happens if a dependency fails? Does it scale properly when it gets more traffic, or does it have a failover zone if one goes out? What is the disaster recovery plan, and can we test that an alert would fire and engineers would know where to go if it happen?
So, I tested out Gremlin. Overall, it was a good tool, but after putting myself in the developer's shoes, it still acts like a SRE tool because it does not integrate into the developer's workflow. It doesn't mean it can't. You can add it through GitHub actions and other tools like Jenkins.<br>

Overall, it was a good experience and I will keep researching that link of testing before merges. 
